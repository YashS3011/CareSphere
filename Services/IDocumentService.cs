using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IDocumentService
    {
        Task<string> GenerateInvoicePdfAsync(Guid invoiceId);
        Task<string> GenerateReceiptPdfAsync(Guid paymentId);
        Task<string> GenerateDischargeBillPdfAsync(Guid encounterId);
        Task<List<InvoiceDocument>> GetDocumentsByInvoiceAsync(Guid invoiceId);
    }
}
