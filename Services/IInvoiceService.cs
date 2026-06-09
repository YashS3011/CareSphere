using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Services
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
    }
}
