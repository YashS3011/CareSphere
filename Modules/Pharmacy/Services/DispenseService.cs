using CareSphere.Modules.Clinical.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Patients.Services;
using CareSphere.Modules.Ward.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Shared.Services;
using CareSphere.Modules.Shared.Events;
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CareSphere.Modules.Shared.Events;

namespace CareSphere.Modules.Pharmacy.Services
{
    public class DispenseService : IDispenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public DispenseService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<DispenseValidationResult> ValidatePrescriptionForDispenseAsync(Guid prescriptionId, string? barcodeScanned = null)
        {
            var result = new DispenseValidationResult { IsValid = true };

            var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
            if (prescription == null)
            {
                result.IsValid = false;
                result.ValidationErrors.Add("Prescription not found.");
                return result;
            }

            if (prescription.Status != "Active")
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Prescription is not active (Status: '{prescription.Status}').");
                return result;
            }

            // Find matching PharmacyItem
            PharmacyItem? item = null;
            if (!string.IsNullOrWhiteSpace(prescription.DrugCode))
            {
                item = await _context.PharmacyItems.FirstOrDefaultAsync(i => i.TenantId == prescription.TenantId && i.ItemCode == prescription.DrugCode);
            }

            if (item == null)
            {
                item = await _context.PharmacyItems.FirstOrDefaultAsync(i => i.TenantId == prescription.TenantId && i.ItemName.ToLower() == prescription.DrugName.ToLower());
            }

            if (item == null)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"No matching pharmacy catalog item found for prescription drug '{prescription.DrugName}' (Code: '{prescription.DrugCode}').");
                return result;
            }

            if (!item.IsActive)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"The matching pharmacy item '{item.ItemName}' is currently inactive.");
                return result;
            }

            // If barcode was scanned, verify match
            if (!string.IsNullOrWhiteSpace(barcodeScanned))
            {
                if (item.Barcode != barcodeScanned)
                {
                    result.IsValid = false;
                    result.ValidationErrors.Add($"Scanned barcode '{barcodeScanned}' does not match the item barcode '{item.Barcode}' for this prescription.");
                }
            }

            // Check how much has already been dispensed
            var totalDispensed = await _context.DispenseRecords
                .Where(r => r.PrescriptionId == prescriptionId)
                .SumAsync(r => r.DispensedQuantity);

            if (totalDispensed >= prescription.Quantity)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Prescription is already fully dispensed. Prescribed: {prescription.Quantity}, Dispensed: {totalDispensed}.");
            }

            // Check stock availability
            var totalStock = await _context.PharmacyBatches
                .Where(b => b.ItemId == item.Id && b.IsActive && b.ExpiryDate > DateTime.UtcNow)
                .SumAsync(b => b.CurrentStock);

            if (totalStock <= 0)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Item '{item.ItemName}' is currently out of stock in the pharmacy.");
            }

            return result;
        }

        public async Task<List<DispenseRecord>> DispenseItemAsync(Guid prescriptionId, int quantityToDispense, string dispensedByUserId, string? barcodeScanned = null)
        {
            if (quantityToDispense <= 0)
                throw new ArgumentException("Quantity to dispense must be greater than zero.", nameof(quantityToDispense));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Re-run validation
                var validation = await ValidatePrescriptionForDispenseAsync(prescriptionId, barcodeScanned);
                if (!validation.IsValid)
                {
                    throw new InvalidOperationException($"Dispensing validation failed: {string.Join(", ", validation.ValidationErrors)}");
                }

                var prescription = (await _context.Prescriptions.FindAsync(prescriptionId))!;

                // Find matching PharmacyItem
                PharmacyItem item = null!;
                if (!string.IsNullOrWhiteSpace(prescription.DrugCode))
                {
                    item = (await _context.PharmacyItems.FirstOrDefaultAsync(i => i.TenantId == prescription.TenantId && i.ItemCode == prescription.DrugCode))!;
                }
                if (item == null)
                {
                    item = (await _context.PharmacyItems.FirstOrDefaultAsync(i => i.TenantId == prescription.TenantId && i.ItemName.ToLower() == prescription.DrugName.ToLower()))!;
                }

                // Check remaining allowed quantity
                var totalDispensedBefore = await _context.DispenseRecords
                    .Where(r => r.PrescriptionId == prescriptionId)
                    .SumAsync(r => r.DispensedQuantity);

                var remainingAllowed = prescription.Quantity - totalDispensedBefore;
                if (quantityToDispense > remainingAllowed)
                {
                    throw new InvalidOperationException($"Cannot dispense {quantityToDispense} units. Only {remainingAllowed} units remain on this prescription.");
                }

                // Get active, non-expired batches order by FEFO
                var batches = await _context.PharmacyBatches
                    .Where(b => b.ItemId == item.Id && b.IsActive && b.ExpiryDate > DateTime.UtcNow && b.CurrentStock > 0)
                    .OrderBy(b => b.ExpiryDate)
                    .ToListAsync();

                var totalAvailableStock = batches.Sum(b => b.CurrentStock);
                if (quantityToDispense > totalAvailableStock)
                {
                    throw new InvalidOperationException($"Insufficient stock. Requested: {quantityToDispense}, Available: {totalAvailableStock}.");
                }

                var dispenseRecords = new List<DispenseRecord>();
                var remainingToDispense = quantityToDispense;

                foreach (var batch in batches)
                {
                    if (remainingToDispense <= 0) break;

                    var toDeduct = Math.Min(remainingToDispense, batch.CurrentStock);
                    batch.CurrentStock -= toDeduct;
                    _context.PharmacyBatches.Update(batch);
                    await _context.SaveChangesAsync();

                    // Calculate balance after
                    var balanceAfter = await _context.PharmacyBatches
                        .Where(b => b.ItemId == item.Id && b.IsActive)
                        .SumAsync(b => b.CurrentStock);

                    // Create ledger entry
                    var ledgerEntry = new StockLedgerEntry
                    {
                        Id = Guid.NewGuid(),
                        TenantId = prescription.TenantId,
                        ItemId = item.Id,
                        BatchId = batch.Id,
                        TransactionType = "Dispensed",
                        ReferenceId = prescription.Id.ToString(),
                        ReferenceType = "Dispense",
                        QuantityIn = 0,
                        QuantityOut = toDeduct,
                        BalanceAfter = balanceAfter,
                        TransactionDate = DateTime.UtcNow,
                        CreatedByUserId = dispensedByUserId,
                        Notes = $"Dispensed for Prescription: {prescription.Id}"
                    };
                    _context.StockLedgerEntries.Add(ledgerEntry);

                    // Create dispense record
                    var totalDispensedSoFar = totalDispensedBefore + (quantityToDispense - remainingToDispense) + toDeduct;
                    var record = new DispenseRecord
                    {
                        Id = Guid.NewGuid(),
                        TenantId = prescription.TenantId,
                        PrescriptionId = prescription.Id,
                        PatientId = prescription.PatientId,
                        ItemId = item.Id,
                        BatchId = batch.Id,
                        DispensedQuantity = toDeduct,
                        DispensedAt = DateTime.UtcNow,
                        DispensedByUserId = dispensedByUserId,
                        BarcodeScanned = barcodeScanned,
                        IsPartialDispense = totalDispensedSoFar < prescription.Quantity,
                        Notes = $"Batch {batch.BatchNumber} used."
                    };
                    _context.DispenseRecords.Add(record);
                    dispenseRecords.Add(record);

                    remainingToDispense -= toDeduct;
                }

                foreach (var record in dispenseRecords)
                {
                    var outboxEvent = new DispenseCompleted
                    {
                        DispenseId = record.Id,
                        PatientId = record.PatientId,
                        TenantId = record.TenantId,
                        PrescriptionId = record.PrescriptionId,
                        ItemName = item.ItemName,
                        Quantity = record.DispensedQuantity
                    };

                    var outbox = new ServiceBusOutbox
                    {
                        Id = Guid.NewGuid(),
                        TenantId = record.TenantId,
                        MessageType = "DispenseCompleted",
                        Payload = JsonSerializer.Serialize(outboxEvent),
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ServiceBusOutboxes.Add(outbox);
                }

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(new AuditEvent
                {
                    UserId = dispensedByUserId,
                    Action = "ITEM_DISPENSED",
                    ResourceType = "Prescription",
                    ResourceId = prescription.Id.ToString(),
                    TenantId = prescription.TenantId
                });

                await transaction.CommitAsync();
                return dispenseRecords;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<DispenseRecord>> GetDispenseHistoryByPrescriptionAsync(Guid prescriptionId)
        {
            return await _context.DispenseRecords
                .Include(r => r.Item)
                .Include(r => r.Batch)
                .Where(r => r.PrescriptionId == prescriptionId)
                .OrderByDescending(r => r.DispensedAt)
                .ToListAsync();
        }

        public async Task<List<DispenseRecord>> GetDispenseHistoryByPatientAsync(Guid patientId)
        {
            return await _context.DispenseRecords
                .Include(r => r.Item)
                .Include(r => r.Batch)
                .Include(r => r.Prescription)
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.DispensedAt)
                .ToListAsync();
        }
    }
}
