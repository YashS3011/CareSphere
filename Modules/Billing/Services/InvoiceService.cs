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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;
using CareSphere.Modules.Shared.Events;
using CareSphere.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace CareSphere.Modules.Billing.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _http;

        private const decimal DefaultConsultationFee = 500.00m;
        private const string ConsultationItemType = "Consultation";
        private const string ConsultationItemCode = "OPD-CONS";
        private const string PharmacyItemType = "Pharmacy";
        private const string InvoiceStatusDraft = "Draft";

        public InvoiceService(ApplicationDbContext context, IAuditService auditService, IHttpContextAccessor http)
        {
            _context = context;
            _auditService = auditService;
            _http = http;
        }

        private string CurrentUserId =>
            _http.HttpContext?.User
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? "system";

        public async Task<BillingInvoice> CreateInvoiceAsync(Guid tenantId, Guid patientId, Guid? encounterId, List<BillingLineItem> lineItems)
        {
            DateTime? admissionDate = null;
            DateTime? dischargeDate = null;

            if (encounterId.HasValue)
            {
                var encounter = await _context.Encounters.FindAsync(encounterId.Value);
                if (encounter != null)
                {
                    admissionDate = encounter.AdmissionDate;
                    dischargeDate = encounter.DischargeDate;
                }
            }

            // Generate unique invoice number: INV-YYYYMMDD-XXXX
            var today = DateTime.UtcNow.Date; // use UTC date for database consistencies
            var prefix = $"INV-{today:yyyyMMdd}-";
            
            var lastInvoice = await _context.BillingInvoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            int seq = 1;
            if (lastInvoice != null)
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                {
                    seq = lastSeq + 1;
                }
            }
            string invoiceNumber = $"{prefix}{seq:D4}";

            var invoice = new BillingInvoice
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceNumber = invoiceNumber,
                PatientId = patientId,
                EncounterId = encounterId,
                AdmissionDate = admissionDate,
                DischargeDate = dischargeDate,
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(15), // Default 15 days due
                Status = "Draft",
                GeneratedByUserId = "system",
                CreatedAt = DateTime.UtcNow,
                Notes = ""
            };

            foreach (var item in lineItems)
            {
                item.Id = Guid.NewGuid();
                item.TenantId = tenantId;
                item.InvoiceId = invoice.Id;
                // Calculate line total: qty * unit price less discount plus tax
                decimal basePrice = item.Quantity * item.UnitPrice;
                decimal discountAmt = basePrice * (item.DiscountPercent / 100m);
                decimal taxableAmt = basePrice - discountAmt;
                decimal taxAmt = taxableAmt * (item.TaxPercent / 100m);
                item.LineTotal = taxableAmt + taxAmt;

                invoice.BillingLineItems.Add(item);
            }

            // Calculate initial totals
            invoice.SubtotalAmount = invoice.BillingLineItems.Sum(li => li.Quantity * li.UnitPrice);
            invoice.DiscountAmount = invoice.BillingLineItems.Sum(li => (li.Quantity * li.UnitPrice) * (li.DiscountPercent / 100m));
            invoice.TaxAmount = invoice.BillingLineItems.Sum(li => ((li.Quantity * li.UnitPrice) - ((li.Quantity * li.UnitPrice) * (li.DiscountPercent / 100m))) * (li.TaxPercent / 100m));
            invoice.TotalAmount = invoice.BillingLineItems.Sum(li => li.LineTotal);
            invoice.PaidAmount = 0m;

            _context.BillingInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Call IAuditService.LogAsync
            // TODO: Replace "system" with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = tenantId,
                UserId = CurrentUserId,
                Action = "INVOICE_CREATED",
                ResourceType = "BillingInvoice",
                ResourceId = invoice.Id.ToString()
            });

            return invoice;
        }

        public async Task<BillingInvoice> AddLineItemAsync(Guid invoiceId, BillingLineItem item)
        {
            var invoice = await _context.BillingInvoices
                .Include(i => i.BillingLineItems)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            if (invoice.Status != "Draft")
                throw new InvalidOperationException("Can only add line items to a Draft invoice.");

            item.Id = Guid.NewGuid();
            item.TenantId = invoice.TenantId;
            item.InvoiceId = invoiceId;

            // Recalculate item line total
            decimal basePrice = item.Quantity * item.UnitPrice;
            decimal discountAmt = basePrice * (item.DiscountPercent / 100m);
            decimal taxableAmt = basePrice - discountAmt;
            decimal taxAmt = taxableAmt * (item.TaxPercent / 100m);
            item.LineTotal = taxableAmt + taxAmt;

            _context.BillingLineItems.Add(item);
            await _context.SaveChangesAsync();

            await UpdateInvoiceTotalsAsync(invoiceId);

            // TODO: Replace "system" with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = invoice.TenantId,
                UserId = CurrentUserId,
                Action = "INVOICE_LINE_ADDED",
                ResourceType = "BillingInvoice",
                ResourceId = invoiceId.ToString()
            });

            return (await GetInvoiceByIdAsync(invoiceId))!;
        }

        public async Task<BillingInvoice> RemoveLineItemAsync(Guid invoiceId, Guid lineItemId)
        {
            var invoice = await _context.BillingInvoices
                .Include(i => i.BillingLineItems)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            if (invoice.Status != "Draft")
                throw new InvalidOperationException("Can only remove line items from a Draft invoice.");

            var item = invoice.BillingLineItems.FirstOrDefault(li => li.Id == lineItemId);
            if (item != null)
            {
                _context.BillingLineItems.Remove(item);
                await _context.SaveChangesAsync();
                await UpdateInvoiceTotalsAsync(invoiceId);

                // TODO: Replace "system" with logged-in user ID once auth is added
                await _auditService.LogAsync(new AuditEvent
                {
                    TenantId = invoice.TenantId,
                    UserId = CurrentUserId,
                    Action = "INVOICE_LINE_REMOVED",
                    ResourceType = "BillingInvoice",
                    ResourceId = invoiceId.ToString()
                });
            }

            return (await GetInvoiceByIdAsync(invoiceId))!;
        }

        public async Task<BillingInvoice> FinalizeInvoiceAsync(Guid invoiceId)
        {
            var invoice = await _context.BillingInvoices.FindAsync(invoiceId);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            await _context.Entry(invoice).ReloadAsync();

            if (invoice.Status != "Draft")
                throw new InvalidOperationException("Only draft invoices can be finalized.");

            invoice.Status = "Finalized";
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log Audit Event
            // TODO: Replace "system" with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = invoice.TenantId,
                UserId = CurrentUserId,
                Action = "INVOICE_FINALIZED",
                ResourceType = "BillingInvoice",
                ResourceId = invoice.Id.ToString()
            });

            return (await GetInvoiceByIdAsync(invoiceId))!;
        }

        public async Task<BillingInvoice?> GetInvoiceByIdAsync(Guid invoiceId)
        {
            return await _context.BillingInvoices.AsNoTracking()
                .Include(i => i.Patient)
                .Include(i => i.Encounter)
                .Include(i => i.BillingLineItems)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
        }

        public async Task<List<BillingInvoice>> GetInvoicesByPatientAsync(Guid patientId)
        {
            return await _context.BillingInvoices.AsNoTracking()
                .Include(i => i.Patient)
                .Where(i => i.PatientId == patientId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<(List<BillingInvoice> Invoices, int TotalCount)> GetInvoicesByStatusAsync(string? status, int page, int pageSize, string searchTerm = "")
        {
            var query = _context.BillingInvoices.AsNoTracking()
                .Include(i => i.Patient)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(i => i.Status == status);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(i => 
                    i.InvoiceNumber.ToLower().Contains(searchTerm) || 
                    i.Patient.FirstName.ToLower().Contains(searchTerm) || 
                    i.Patient.LastName.ToLower().Contains(searchTerm) ||
                    i.Patient.Mrn.ToLower().Contains(searchTerm));
            }

            int totalCount = await query.CountAsync();
            var invoices = await query
                .OrderByDescending(i => i.InvoiceDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (invoices, totalCount);
        }

        public async Task<BillingInvoice> CancelInvoiceAsync(Guid invoiceId)
        {
            var invoice = await _context.BillingInvoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            // Can cancel only if no successful payments exist
            bool hasSuccessPayment = invoice.Payments.Any(p => p.Status == "Success");
            if (hasSuccessPayment)
                throw new InvalidOperationException("Cannot cancel an invoice with successful payments.");

            invoice.Status = "Cancelled";
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log Audit Event
            // TODO: Replace "system" with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = invoice.TenantId,
                UserId = CurrentUserId,
                Action = "INVOICE_CANCELLED",
                ResourceType = "BillingInvoice",
                ResourceId = invoice.Id.ToString()
            });

            return (await GetInvoiceByIdAsync(invoiceId))!;
        }

        public async Task UpdateInvoiceTotalsAsync(Guid invoiceId)
        {
            var invoice = await _context.BillingInvoices
                .Include(i => i.BillingLineItems)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                return;

            // Update item totals first just in case
            foreach (var item in invoice.BillingLineItems)
            {
                decimal basePrice = item.Quantity * item.UnitPrice;
                decimal discountAmt = basePrice * (item.DiscountPercent / 100m);
                decimal taxableAmt = basePrice - discountAmt;
                decimal taxAmt = taxableAmt * (item.TaxPercent / 100m);
                item.LineTotal = taxableAmt + taxAmt;
            }

            invoice.SubtotalAmount = invoice.BillingLineItems.Sum(li => li.Quantity * li.UnitPrice);
            invoice.DiscountAmount = invoice.BillingLineItems.Sum(li => (li.Quantity * li.UnitPrice) * (li.DiscountPercent / 100m));
            invoice.TaxAmount = invoice.BillingLineItems.Sum(li => ((li.Quantity * li.UnitPrice) - ((li.Quantity * li.UnitPrice) * (li.DiscountPercent / 100m))) * (li.TaxPercent / 100m));
            invoice.TotalAmount = invoice.BillingLineItems.Sum(li => li.LineTotal);
            invoice.PaidAmount = invoice.Payments
                .Where(p => p.Status == "Success")
                .Sum(p => p.Amount);

            // In DB BalanceAmount is computed, but we can set it in model for in-memory consistencies if needed
            invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;

            // Auto-adjust Status if finalized or partially paid
            if (invoice.Status != "Draft" && invoice.Status != "Cancelled")
            {
                if (invoice.PaidAmount >= invoice.TotalAmount)
                {
                    invoice.Status = "Paid";
                }
                else if (invoice.PaidAmount > 0)
                {
                    invoice.Status = "PartiallyPaid";
                }
                else
                {
                    invoice.Status = "Finalized";
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task CreateDraftFromEncounterAsync(EncounterCompleted evt)
        {
            var originalBypassId = TenantContext.BypassTenantId;
            TenantContext.BypassTenantId = evt.TenantId;
            try
            {
                var existingInvoice = await _context.BillingInvoices
                    .FirstOrDefaultAsync(i => i.TenantId == evt.TenantId && i.EncounterId == evt.EncounterId && i.Status == InvoiceStatusDraft);

                if (existingInvoice != null)
                {
                    return;
                }

                var encounter = await _context.Encounters
                    .Include(e => e.Doctor)
                    .FirstOrDefaultAsync(e => e.TenantId == evt.TenantId && e.Id == evt.EncounterId);

                if (encounter == null)
                {
                    throw new KeyNotFoundException($"Encounter with ID {evt.EncounterId} was not found.");
                }

                var itemDescription = encounter.Doctor != null
                    ? $"{encounter.Doctor.Specialization} Fee (Dr. {encounter.Doctor.FirstName} {encounter.Doctor.LastName})"
                    : "OPD Consultation Fee";

                var lineItem = new BillingLineItem
                {
                    TenantId = evt.TenantId,
                    ItemType = ConsultationItemType,
                    ItemCode = ConsultationItemCode,
                    ItemDescription = itemDescription,
                    Quantity = 1m,
                    UnitPrice = DefaultConsultationFee,
                    LineTotal = DefaultConsultationFee
                };

                await CreateInvoiceAsync(evt.TenantId, evt.PatientId, evt.EncounterId, new List<BillingLineItem> { lineItem });
            }
            finally
            {
                TenantContext.BypassTenantId = originalBypassId;
            }
        }

        public async Task AddDispenseLineItemAsync(DispenseCompleted evt)
        {
            var originalBypassId = TenantContext.BypassTenantId;
            TenantContext.BypassTenantId = evt.TenantId;
            try
            {
                var dispense = await _context.DispenseRecords
                    .Include(d => d.Item)
                    .Include(d => d.Batch)
                    .FirstOrDefaultAsync(d => d.TenantId == evt.TenantId && d.Id == evt.DispenseId);

                if (dispense == null)
                {
                    throw new KeyNotFoundException($"DispenseRecord with ID {evt.DispenseId} was not found.");
                }

                var prescription = await _context.Prescriptions
                    .FirstOrDefaultAsync(p => p.TenantId == evt.TenantId && p.Id == evt.PrescriptionId);

                if (prescription == null || prescription.EncounterId == Guid.Empty)
                {
                    return;
                }

                var invoice = await _context.BillingInvoices
                    .Include(i => i.BillingLineItems)
                    .FirstOrDefaultAsync(i => i.TenantId == evt.TenantId && i.PatientId == evt.PatientId && i.EncounterId == prescription.EncounterId && i.Status == InvoiceStatusDraft);

                if (invoice == null)
                {
                    invoice = await CreateInvoiceAsync(evt.TenantId, evt.PatientId, prescription.EncounterId, new List<BillingLineItem>());
                }

                decimal unitPrice = dispense.Batch?.SellingPrice ?? 0m;
                if (dispense.Batch == null || dispense.Batch.SellingPrice == 0m)
                {
                    Console.WriteLine($"[Warning] Missing selling price or batch for dispense record {dispense.Id}. Defaulting unit price to 0.");
                }

                var itemDescription = dispense.Item != null && dispense.Batch != null
                    ? $"{dispense.Item.ItemName} (Batch: {dispense.Batch.BatchNumber})"
                    : (dispense.Item != null ? dispense.Item.ItemName : "Unknown Medication");

                var lineItem = new BillingLineItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = evt.TenantId,
                    InvoiceId = invoice.Id,
                    ItemType = PharmacyItemType,
                    ItemDescription = itemDescription,
                    ItemCode = dispense.Item?.ItemCode,
                    Quantity = dispense.DispensedQuantity,
                    UnitPrice = unitPrice,
                    DiscountPercent = 0m,
                    TaxPercent = 0m,
                    LineTotal = dispense.DispensedQuantity * unitPrice
                };

                _context.BillingLineItems.Add(lineItem);
                await _context.SaveChangesAsync();

                await UpdateInvoiceTotalsAsync(invoice.Id);
            }
            finally
            {
                TenantContext.BypassTenantId = originalBypassId;
            }
        }
        public async Task AddLabRequisitionLineItemsAsync(LabRequisitionCreated evt)
        {
            var originalBypassId = TenantContext.BypassTenantId;
            TenantContext.BypassTenantId = evt.TenantId;
            try
            {
                if (evt.Tests == null || !evt.Tests.Any())
                    return;

                // Find the draft invoice for this encounter (or patient if no encounter)
                BillingInvoice? invoice = null;
                if (evt.EncounterId.HasValue)
                {
                    invoice = await _context.BillingInvoices
                        .Include(i => i.BillingLineItems)
                        .FirstOrDefaultAsync(i => i.TenantId == evt.TenantId
                                               && i.EncounterId == evt.EncounterId
                                               && i.Status == InvoiceStatusDraft);
                }

                if (invoice == null)
                {
                    // Create a new draft invoice for these lab charges
                    var lineItems = evt.Tests
                        .Where(t => t.Fee > 0)
                        .Select(t => new BillingLineItem
                        {
                            TenantId = evt.TenantId,
                            ItemType = "Laboratory",
                            ItemCode = t.TestCode,
                            ItemDescription = $"Lab Test: {t.TestName} (Req: {evt.RequisitionNumber})",
                            Quantity = 1m,
                            UnitPrice = t.Fee,
                            DiscountPercent = 0m,
                            TaxPercent = 0m,
                            LineTotal = t.Fee
                        }).ToList();

                    if (lineItems.Any())
                        await CreateInvoiceAsync(evt.TenantId, evt.PatientId, evt.EncounterId, lineItems);

                    return;
                }

                // Add line items to existing draft invoice
                foreach (var test in evt.Tests.Where(t => t.Fee > 0))
                {
                    var lineItem = new BillingLineItem
                    {
                        Id = Guid.NewGuid(),
                        TenantId = evt.TenantId,
                        InvoiceId = invoice.Id,
                        ItemType = "Laboratory",
                        ItemCode = test.TestCode,
                        ItemDescription = $"Lab Test: {test.TestName} (Req: {evt.RequisitionNumber})",
                        Quantity = 1m,
                        UnitPrice = test.Fee,
                        DiscountPercent = 0m,
                        TaxPercent = 0m,
                        LineTotal = test.Fee
                    };
                    _context.BillingLineItems.Add(lineItem);
                }

                await _context.SaveChangesAsync();
                await UpdateInvoiceTotalsAsync(invoice.Id);
            }
            finally
            {
                TenantContext.BypassTenantId = originalBypassId;
            }
        }

        public async Task AddDailyBedChargeAsync(Guid tenantId, Guid patientId, Guid? encounterId, decimal chargeAmount, string bedNumber)
        {
            var originalBypassId = TenantContext.BypassTenantId;
            TenantContext.BypassTenantId = tenantId;
            try
            {
                if (chargeAmount <= 0)
                    return;

                BillingInvoice? invoice = null;
                if (encounterId.HasValue)
                {
                    invoice = await _context.BillingInvoices
                        .Include(i => i.BillingLineItems)
                        .FirstOrDefaultAsync(i => i.TenantId == tenantId
                                               && i.EncounterId == encounterId.Value
                                               && i.Status == InvoiceStatusDraft);
                }

                if (invoice == null)
                {
                    invoice = await _context.BillingInvoices
                        .Include(i => i.BillingLineItems)
                        .FirstOrDefaultAsync(i => i.TenantId == tenantId
                                               && i.PatientId == patientId
                                               && i.Status == InvoiceStatusDraft);
                }

                if (invoice == null)
                {
                    var lineItem = new BillingLineItem
                    {
                        TenantId = tenantId,
                        ItemType = "BedCharge",
                        ItemCode = "BED-" + bedNumber,
                        ItemDescription = $"Daily Bed Charge: Bed {bedNumber}",
                        Quantity = 1m,
                        UnitPrice = chargeAmount,
                        DiscountPercent = 0m,
                        TaxPercent = 0m,
                        LineTotal = chargeAmount
                    };

                    await CreateInvoiceAsync(tenantId, patientId, encounterId, new List<BillingLineItem> { lineItem });
                }
                else
                {
                    var lineItem = new BillingLineItem
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        InvoiceId = invoice.Id,
                        ItemType = "BedCharge",
                        ItemCode = "BED-" + bedNumber,
                        ItemDescription = $"Daily Bed Charge: Bed {bedNumber}",
                        Quantity = 1m,
                        UnitPrice = chargeAmount,
                        DiscountPercent = 0m,
                        TaxPercent = 0m,
                        LineTotal = chargeAmount
                    };

                    _context.BillingLineItems.Add(lineItem);
                    await _context.SaveChangesAsync();
                    await UpdateInvoiceTotalsAsync(invoice.Id);
                }
            }
            finally
            {
                TenantContext.BypassTenantId = originalBypassId;
            }
        }
    }
}
