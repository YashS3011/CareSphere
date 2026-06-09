using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;
using CareSphere.Infrastructure;

namespace CareSphere.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly RazorpayClientWrapper _razorpayClient;
        private readonly IInvoiceService _invoiceService;
        private readonly IAuditService _auditService;

        public PaymentService(
            ApplicationDbContext context, 
            RazorpayClientWrapper razorpayClient, 
            IInvoiceService invoiceService, 
            IAuditService auditService)
        {
            _context = context;
            _razorpayClient = razorpayClient;
            _invoiceService = invoiceService;
            _auditService = auditService;
        }

        public async Task<Payment> RecordCashPaymentAsync(Guid tenantId, Guid invoiceId, decimal amount, string? notes, string recordedByUserId)
        {
            var invoice = await _context.BillingInvoices.FindAsync(invoiceId);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            if (invoice.Status == "Draft" || invoice.Status == "Cancelled")
                throw new InvalidOperationException("Cannot record payments on Draft or Cancelled invoices.");

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceId = invoiceId,
                PatientId = invoice.PatientId,
                PaymentDate = DateTime.UtcNow,
                Amount = amount,
                PaymentMethod = "Cash",
                Status = "Success",
                Notes = notes,
                RecordedByUserId = recordedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Recalculate totals
            await _invoiceService.UpdateInvoiceTotalsAsync(invoiceId);

            // Log Audit Event
            // TODO: Replace recordedByUserId with actual logged-in user ID
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = tenantId,
                UserId = recordedByUserId,
                Action = "PAYMENT_RECORDED",
                ResourceType = "Payment",
                ResourceId = payment.Id.ToString()
            });

            return payment;
        }

        public async Task<RazorpayOrderResult> InitiateRazorpayPaymentAsync(Guid tenantId, Guid invoiceId, string paymentMethod)
        {
            var invoice = await _context.BillingInvoices.FindAsync(invoiceId);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            if (invoice.Status == "Draft" || invoice.Status == "Cancelled")
                throw new InvalidOperationException("Cannot initiate payment on Draft or Cancelled invoices.");

            // Initiate Order via wrapper
            var razorpayOrder = await _razorpayClient.CreateOrderAsync(invoice.BalanceAmount, invoice.InvoiceNumber);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceId = invoiceId,
                PatientId = invoice.PatientId,
                PaymentDate = DateTime.UtcNow,
                Amount = invoice.BalanceAmount,
                PaymentMethod = paymentMethod, // UPI or Card
                RazorpayOrderId = razorpayOrder.OrderId,
                Status = "Pending",
                RecordedByUserId = "system",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Log Audit Event
            // TODO: Replace with actual logged-in user ID
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = tenantId,
                UserId = "system",
                Action = "PAYMENT_INITIATED",
                ResourceType = "Payment",
                ResourceId = payment.Id.ToString()
            });

            return razorpayOrder;
        }

        public async Task<Payment> VerifyRazorpayPaymentAsync(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.RazorpayOrderId == razorpayOrderId);

            if (payment == null)
                throw new KeyNotFoundException("Payment record for the specified order not found.");

            bool isVerified = _razorpayClient.VerifySignature(razorpayOrderId, razorpayPaymentId, razorpaySignature);

            payment.RazorpayPaymentId = razorpayPaymentId;
            payment.RazorpaySignature = razorpaySignature;
            payment.Status = isVerified ? "Success" : "Failed";

            await _context.SaveChangesAsync();

            if (isVerified)
            {
                // Update Invoice totals
                await _invoiceService.UpdateInvoiceTotalsAsync(payment.InvoiceId);
            }

            // Log Audit Event
            // TODO: Replace with actual logged-in user ID
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = payment.TenantId,
                UserId = "system",
                Action = "PAYMENT_VERIFIED",
                ResourceType = "Payment",
                ResourceId = payment.Id.ToString()
            });

            return payment;
        }

        public async Task<List<Payment>> GetPaymentsByInvoiceAsync(Guid invoiceId)
        {
            return await _context.Payments
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment> RecordInsuranceSettlementAsync(Guid tenantId, Guid invoiceId, Guid claimId, decimal amount, string? transactionReference, string recordedByUserId)
        {
            var invoice = await _context.BillingInvoices.FindAsync(invoiceId);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            var claim = await _context.InsuranceClaims.FindAsync(claimId);
            if (claim == null)
                throw new KeyNotFoundException("Insurance claim not found.");

            if (invoice.Status == "Draft" || invoice.Status == "Cancelled")
                throw new InvalidOperationException("Cannot settle Draft or Cancelled invoices.");

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceId = invoiceId,
                PatientId = invoice.PatientId,
                PaymentDate = DateTime.UtcNow,
                Amount = amount,
                PaymentMethod = "Insurance",
                TransactionReference = transactionReference ?? claim.ClaimNumber,
                Status = "Success",
                Notes = $"Settlement for Claim {claim.ClaimNumber}",
                RecordedByUserId = recordedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // Update Claim status to Approved or PartiallyApproved
            claim.Status = amount >= claim.ClaimedAmount ? "Approved" : "PartiallyApproved";
            claim.ApprovedAmount = amount;
            claim.ApprovedAt = DateTime.UtcNow;
            claim.UpdatedAt = DateTime.UtcNow;

            // Log Claim status history
            var history = new ClaimStatusHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClaimId = claimId,
                PreviousStatus = "Submitted",
                NewStatus = claim.Status,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = recordedByUserId,
                Remarks = $"Insurance settlement recorded of INR {amount:N2}. Transaction Ref: {transactionReference}"
            };
            _context.ClaimStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            // Recalculate invoice totals
            await _invoiceService.UpdateInvoiceTotalsAsync(invoiceId);

            // Log Audit Events
            // TODO: Replace with logged-in user ID
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = tenantId,
                UserId = recordedByUserId,
                Action = "PAYMENT_RECORDED",
                ResourceType = "Payment",
                ResourceId = payment.Id.ToString()
            });

            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = tenantId,
                UserId = recordedByUserId,
                Action = "CLAIM_STATUS_UPDATED",
                ResourceType = "InsuranceClaim",
                ResourceId = claimId.ToString()
            });

            return payment;
        }
    }
}
