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
using System.Threading.Tasks;
using CareSphere.Models;
using CareSphere.Infrastructure;

namespace CareSphere.Modules.Billing.Services
{
    public interface IPaymentService
    {
        Task<Payment> RecordCashPaymentAsync(Guid tenantId, Guid invoiceId, decimal amount, string? notes, string recordedByUserId);
        Task<RazorpayOrderResult> InitiateRazorpayPaymentAsync(Guid tenantId, Guid invoiceId, string paymentMethod);
        Task<Payment> VerifyRazorpayPaymentAsync(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
        Task<List<Payment>> GetPaymentsByInvoiceAsync(Guid invoiceId);
        Task<Payment> RecordInsuranceSettlementAsync(Guid tenantId, Guid invoiceId, Guid claimId, decimal amount, string? transactionReference, string recordedByUserId);
    }
}
