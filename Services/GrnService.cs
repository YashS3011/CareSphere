using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class GrnService : IGrnService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public GrnService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        /// <summary>
        /// Ensures a DateTime has Kind=Utc. If Kind is Unspecified or Local, converts to UTC.
        /// </summary>
        private static DateTime NormalizeToUtc(DateTime dt)
        {
            return dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc) // Unspecified → treat as UTC
            };
        }

        private static DateTime? NormalizeToUtc(DateTime? dt)
        {
            return dt.HasValue ? NormalizeToUtc(dt.Value) : null;
        }

        public async Task<GoodsReceivedNote> CreateGrnAsync(GoodsReceivedNote grn)
        {
            grn.Id = Guid.NewGuid();
            grn.CreatedAt = DateTime.UtcNow;
            grn.Status = "Draft";
            grn.ReceivedDate = NormalizeToUtc(grn.ReceivedDate);

            // Generate GRN Number: GRN-YYYYMMDD-XXXX
            var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var countToday = await _context.GoodsReceivedNotes
                .CountAsync(g => g.TenantId == grn.TenantId && g.GrnNumber.StartsWith($"GRN-{todayStr}-"));
            grn.GrnNumber = $"GRN-{todayStr}-{(countToday + 1):D4}";

            decimal total = 0;
            foreach (var item in grn.Items)
            {
                item.Id = Guid.NewGuid();
                item.GrnId = grn.Id;
                item.TenantId = grn.TenantId;
                item.ExpiryDate = NormalizeToUtc(item.ExpiryDate);
                item.ManufactureDate = NormalizeToUtc(item.ManufactureDate);
                item.TotalAmount = item.ReceivedQuantity * item.PurchasePrice;
                total += item.TotalAmount;
            }
            grn.TotalAmount = total;

            _context.GoodsReceivedNotes.Add(grn);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = grn.ReceivedByUserId,
                Action = "GRN_CREATED",
                ResourceType = "GoodsReceivedNote",
                ResourceId = grn.Id.ToString(),
                TenantId = grn.TenantId
            });

            return grn;
        }

        public async Task<GoodsReceivedNote?> GetGrnByIdAsync(Guid id)
        {
            return await _context.GoodsReceivedNotes
                .Include(g => g.Supplier)
                .Include(g => g.PurchaseOrder)
                .Include(g => g.Items)
                    .ThenInclude(gi => gi.Item)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<GoodsReceivedNote>> GetGrnsAsync(Guid tenantId, Guid? supplierId, string? status, int page = 1, int pageSize = 10)
        {
            var query = _context.GoodsReceivedNotes
                .Include(g => g.Supplier)
                .Where(g => g.TenantId == tenantId);

            if (supplierId.HasValue && supplierId.Value != Guid.Empty)
            {
                query = query.Where(g => g.SupplierId == supplierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(g => g.Status == status);
            }

            return await query
                .OrderByDescending(g => g.ReceivedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetGrnsCountAsync(Guid tenantId, Guid? supplierId, string? status)
        {
            var query = _context.GoodsReceivedNotes.Where(g => g.TenantId == tenantId);

            if (supplierId.HasValue && supplierId.Value != Guid.Empty)
            {
                query = query.Where(g => g.SupplierId == supplierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(g => g.Status == status);
            }

            return await query.CountAsync();
        }

        public async Task<GoodsReceivedNote> UpdateGrnAsync(GoodsReceivedNote grn)
        {
            var existing = await _context.GoodsReceivedNotes
                .Include(g => g.Items)
                .FirstOrDefaultAsync(g => g.Id == grn.Id);

            if (existing == null)
                throw new InvalidOperationException("GRN not found.");

            if (existing.Status != "Draft")
                throw new InvalidOperationException("Only draft GRNs can be modified.");

            existing.SupplierId = grn.SupplierId;
            existing.PoId = grn.PoId;
            existing.ReceivedDate = NormalizeToUtc(grn.ReceivedDate);
            existing.InvoiceNumber = grn.InvoiceNumber;
            existing.Notes = grn.Notes;

            // Remove items not present in the new list
            var newItemIds = grn.Items.Select(i => i.Id).ToList();
            var itemsToRemove = existing.Items.Where(i => !newItemIds.Contains(i.Id)).ToList();
            _context.GrnItems.RemoveRange(itemsToRemove);

            // Update/Add items
            decimal total = 0;
            foreach (var inputItem in grn.Items)
            {
                inputItem.TotalAmount = inputItem.ReceivedQuantity * inputItem.PurchasePrice;
                total += inputItem.TotalAmount;

                var existingItem = existing.Items.FirstOrDefault(i => i.Id == inputItem.Id);
                if (existingItem != null)
                {
                    existingItem.ItemId = inputItem.ItemId;
                    existingItem.BatchNumber = inputItem.BatchNumber;
                    existingItem.ManufactureDate = NormalizeToUtc(inputItem.ManufactureDate);
                    existingItem.ExpiryDate = NormalizeToUtc(inputItem.ExpiryDate);
                    existingItem.ReceivedQuantity = inputItem.ReceivedQuantity;
                    existingItem.FreeQuantity = inputItem.FreeQuantity;
                    existingItem.PurchasePrice = inputItem.PurchasePrice;
                    existingItem.SellingPrice = inputItem.SellingPrice;
                    existingItem.TotalAmount = inputItem.TotalAmount;
                }
                else
                {
                    inputItem.Id = Guid.NewGuid();
                    inputItem.GrnId = existing.Id;
                    inputItem.TenantId = existing.TenantId;
                    inputItem.ExpiryDate = NormalizeToUtc(inputItem.ExpiryDate);
                    inputItem.ManufactureDate = NormalizeToUtc(inputItem.ManufactureDate);
                    existing.Items.Add(inputItem);
                }
            }

            existing.TotalAmount = total;
            _context.GoodsReceivedNotes.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<GoodsReceivedNote> PostGrnAsync(Guid grnId)
        {
            // Use transaction to ensure consistency
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var grn = await _context.GoodsReceivedNotes
                    .Include(g => g.Items)
                    .FirstOrDefaultAsync(g => g.Id == grnId);

                if (grn == null)
                    throw new InvalidOperationException("Goods Received Note not found.");

                if (grn.Status == "Posted")
                    throw new InvalidOperationException("GRN is already posted.");

                grn.Status = "Posted";

                // Resolve PO if attached
                PurchaseOrder? po = null;
                if (grn.PoId.HasValue)
                {
                    po = await _context.PurchaseOrders
                        .Include(p => p.Items)
                        .FirstOrDefaultAsync(p => p.Id == grn.PoId.Value);
                }

                foreach (var grnItem in grn.Items)
                {
                    // 1. Process Batch (Find existing active batch or create new one)
                    var batch = await _context.PharmacyBatches
                        .FirstOrDefaultAsync(b => b.TenantId == grn.TenantId &&
                                                 b.ItemId == grnItem.ItemId &&
                                                 b.BatchNumber.ToLower() == grnItem.BatchNumber.ToLower() &&
                                                 b.IsActive);

                    var totalReceived = grnItem.ReceivedQuantity + grnItem.FreeQuantity;

                    if (batch != null)
                    {
                        // Update stock in existing batch
                        batch.CurrentStock += totalReceived;
                        batch.PurchasePrice = grnItem.PurchasePrice;
                        batch.SellingPrice = grnItem.SellingPrice;
                        batch.ExpiryDate = NormalizeToUtc(grnItem.ExpiryDate); // Update expiry if changed
                        if (grnItem.ManufactureDate.HasValue)
                            batch.ManufactureDate = NormalizeToUtc(grnItem.ManufactureDate);

                        _context.PharmacyBatches.Update(batch);
                    }
                    else
                    {
                        // Create new batch
                        batch = new PharmacyBatch
                        {
                            Id = Guid.NewGuid(),
                            TenantId = grn.TenantId,
                            ItemId = grnItem.ItemId,
                            BatchNumber = grnItem.BatchNumber,
                            SupplierId = grn.SupplierId,
                            ManufactureDate = NormalizeToUtc(grnItem.ManufactureDate),
                            ExpiryDate = NormalizeToUtc(grnItem.ExpiryDate),
                            PurchasePrice = grnItem.PurchasePrice,
                            SellingPrice = grnItem.SellingPrice,
                            CurrentStock = totalReceived,
                            ReservedStock = 0,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.PharmacyBatches.Add(batch);
                    }

                    // Save batch to DB so it has an Id if new
                    await _context.SaveChangesAsync();

                    grnItem.BatchId = batch.Id;
                    _context.GrnItems.Update(grnItem);

                    // 2. Update stock ledger & compute BalanceAfter
                    // Calculate total stock of this item across all active batches AFTER the batch update is saved
                    var itemTotalStock = await _context.PharmacyBatches
                        .Where(b => b.ItemId == grnItem.ItemId && b.IsActive)
                        .SumAsync(b => b.CurrentStock);

                    var ledgerEntry = new StockLedgerEntry
                    {
                        Id = Guid.NewGuid(),
                        TenantId = grn.TenantId,
                        ItemId = grnItem.ItemId,
                        BatchId = batch.Id,
                        TransactionType = "GRN",
                        ReferenceId = grn.GrnNumber,
                        ReferenceType = "GRN",
                        QuantityIn = totalReceived,
                        QuantityOut = 0,
                        BalanceAfter = itemTotalStock,
                        TransactionDate = DateTime.UtcNow,
                        CreatedByUserId = grn.ReceivedByUserId,
                        Notes = $"Received via GRN: {grn.GrnNumber}"
                    };

                    _context.StockLedgerEntries.Add(ledgerEntry);

                    // 3. Update Purchase Order Item received quantity
                    if (po != null)
                    {
                        var poItem = po.Items.FirstOrDefault(pi => pi.ItemId == grnItem.ItemId);
                        if (poItem != null)
                        {
                            poItem.ReceivedQuantity += grnItem.ReceivedQuantity;
                            _context.PurchaseOrderItems.Update(poItem);
                        }
                    }
                }

                // 4. Update PO Status
                if (po != null)
                {
                    var allFullyReceived = po.Items.All(pi => pi.ReceivedQuantity >= pi.RequestedQuantity);
                    var anyReceived = po.Items.Any(pi => pi.ReceivedQuantity > 0);

                    if (allFullyReceived)
                    {
                        po.Status = "Received";
                    }
                    else if (anyReceived)
                    {
                        po.Status = "PartiallyReceived";
                    }
                    else
                    {
                        po.Status = "Sent";
                    }

                    po.UpdatedAt = DateTime.UtcNow;
                    _context.PurchaseOrders.Update(po);
                }

                _context.GoodsReceivedNotes.Update(grn);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(new AuditEvent
                {
                    UserId = grn.ReceivedByUserId,
                    Action = "GRN_POSTED",
                    ResourceType = "GoodsReceivedNote",
                    ResourceId = grn.Id.ToString(),
                    TenantId = grn.TenantId
                });

                await transaction.CommitAsync();
                return grn;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
