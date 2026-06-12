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
using CareSphere.Modules.Shared.Events;

namespace CareSphere.Modules.Billing.Services
{
    public interface IInvoiceService
    {
        Task<BillingInvoice> CreateInvoiceAsync(Guid tenantId, Guid patientId, Guid? encounterId, List<BillingLineItem> lineItems);
        Task<BillingInvoice> AddLineItemAsync(Guid invoiceId, BillingLineItem item);
        Task<BillingInvoice> RemoveLineItemAsync(Guid invoiceId, Guid lineItemId);
        Task<BillingInvoice> FinalizeInvoiceAsync(Guid invoiceId);
        Task<BillingInvoice?> GetInvoiceByIdAsync(Guid invoiceId);
        Task<List<BillingInvoice>> GetInvoicesByPatientAsync(Guid patientId);
        Task<(List<BillingInvoice> Invoices, int TotalCount)> GetInvoicesByStatusAsync(string? status, int page, int pageSize, string searchTerm = "");
        Task<BillingInvoice> CancelInvoiceAsync(Guid invoiceId);
        Task UpdateInvoiceTotalsAsync(Guid invoiceId);
        Task CreateDraftFromEncounterAsync(EncounterCompleted evt);
        Task AddDispenseLineItemAsync(DispenseCompleted evt);
    }
}
