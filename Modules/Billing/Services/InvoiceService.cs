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

namespace CareSphere.Modules.Billing.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public InvoiceService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

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
                UserId = "system",
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
                UserId = "system",
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
                    UserId = "system",
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
                UserId = "system",
                Action = "INVOICE_FINALIZED",
                ResourceType = "BillingInvoice",
                ResourceId = invoice.Id.ToString()
            });

            return (await GetInvoiceByIdAsync(invoiceId))!;
        }

        public async Task<BillingInvoice?> GetInvoiceByIdAsync(Guid invoiceId)
        {
            return await _context.BillingInvoices
                .Include(i => i.Patient)
                .Include(i => i.Encounter)
                .Include(i => i.BillingLineItems)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
        }

        public async Task<List<BillingInvoice>> GetInvoicesByPatientAsync(Guid patientId)
        {
            return await _context.BillingInvoices
                .Include(i => i.Patient)
                .Where(i => i.PatientId == patientId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<(List<BillingInvoice> Invoices, int TotalCount)> GetInvoicesByStatusAsync(string? status, int page, int pageSize, string searchTerm = "")
        {
            var query = _context.BillingInvoices
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
                UserId = "system",
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

        public Task CreateDraftFromEncounterAsync(EncounterCompleted evt)
        {
            // TODO: Implement draft invoice generation from completed encounter
            return Task.CompletedTask;
        }

        public Task AddDispenseLineItemAsync(DispenseCompleted evt)
        {
            // TODO: Implement billing line item addition from completed pharmacy dispense
            return Task.CompletedTask;
        }
    }
}
