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

namespace CareSphere.Modules.Billing.Services
{
    public interface IDocumentService
    {
        Task<string> GenerateInvoicePdfAsync(Guid invoiceId);
        Task<string> GenerateReceiptPdfAsync(Guid paymentId);
        Task<string> GenerateDischargeBillPdfAsync(Guid encounterId);
        Task<List<InvoiceDocument>> GetDocumentsByInvoiceAsync(Guid invoiceId);
    }
}
