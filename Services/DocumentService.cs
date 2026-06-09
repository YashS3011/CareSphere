using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using CareSphere.Data;
using CareSphere.Models;
using CareSphere.Documents;

namespace CareSphere.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DocumentService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> GenerateInvoicePdfAsync(Guid invoiceId)
        {
            // Replace local storage with Supabase Storage when configured
            var invoice = await _context.BillingInvoices
                .Include(i => i.Patient)
                .Include(i => i.Encounter)
                .Include(i => i.BillingLineItems)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "documents", "invoices");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"INV-{invoice.InvoiceNumber}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);
            string relativePath = $"documents/invoices/{fileName}";

            // Compile PDF
            var pdfDoc = new CareSphere.Documents.InvoiceDocument(invoice);
            pdfDoc.GeneratePdf(fullPath);

            long fileSizeBytes = new FileInfo(fullPath).Length;

            // Mark previous invoice documents as not latest
            var previousDocs = await _context.InvoiceDocuments
                .Where(d => d.InvoiceId == invoiceId && d.DocumentType == "Invoice")
                .ToListAsync();
            foreach (var doc in previousDocs)
            {
                doc.IsLatest = false;
            }

            var docRecord = new CareSphere.Models.InvoiceDocument
            {
                Id = Guid.NewGuid(),
                TenantId = invoice.TenantId,
                InvoiceId = invoiceId,
                DocumentType = "Invoice",
                StoragePath = relativePath,
                FileName = fileName,
                GeneratedAt = DateTime.UtcNow,
                FileSizeBytes = fileSizeBytes,
                IsLatest = true
            };

            _context.InvoiceDocuments.Add(docRecord);
            await _context.SaveChangesAsync();

            return relativePath;
        }

        public async Task<string> GenerateReceiptPdfAsync(Guid paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Patient)
                .Include(p => p.BillingInvoice)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new KeyNotFoundException("Payment record not found.");

            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "documents", "receipts");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"REC-{paymentId}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);
            string relativePath = $"documents/receipts/{fileName}";

            // Compile PDF
            var pdfDoc = new CareSphere.Documents.ReceiptDocument(payment);
            pdfDoc.GeneratePdf(fullPath);

            long fileSizeBytes = new FileInfo(fullPath).Length;

            // Mark previous receipt documents for this invoice as not latest
            var previousDocs = await _context.InvoiceDocuments
                .Where(d => d.InvoiceId == payment.InvoiceId && d.DocumentType == "Receipt")
                .ToListAsync();
            foreach (var doc in previousDocs)
            {
                doc.IsLatest = false;
            }

            var docRecord = new CareSphere.Models.InvoiceDocument
            {
                Id = Guid.NewGuid(),
                TenantId = payment.TenantId,
                InvoiceId = payment.InvoiceId,
                DocumentType = "Receipt",
                StoragePath = relativePath,
                FileName = fileName,
                GeneratedAt = DateTime.UtcNow,
                FileSizeBytes = fileSizeBytes,
                IsLatest = true
            };

            _context.InvoiceDocuments.Add(docRecord);
            await _context.SaveChangesAsync();

            return relativePath;
        }

        public async Task<string> GenerateDischargeBillPdfAsync(Guid encounterId)
        {
            var encounter = await _context.Encounters
                .Include(e => e.Patient)
                .FirstOrDefaultAsync(e => e.Id == encounterId);

            if (encounter == null)
                throw new KeyNotFoundException("Encounter not found.");

            var invoices = await _context.BillingInvoices
                .Include(i => i.BillingLineItems)
                .Where(i => i.EncounterId == encounterId)
                .ToListAsync();

            if (!invoices.Any())
                throw new InvalidOperationException("No invoices associated with this encounter to generate a bill.");

            var mainInvoice = invoices.First();

            var claims = await _context.InsuranceClaims
                .Where(c => c.EncounterId == encounterId)
                .ToListAsync();

            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "documents", "discharge");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"DCB-{encounterId}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);
            string relativePath = $"documents/discharge/{fileName}";

            // Compile PDF
            var pdfDoc = new CareSphere.Documents.DischargeBillDocument(encounter, invoices, claims);
            pdfDoc.GeneratePdf(fullPath);

            long fileSizeBytes = new FileInfo(fullPath).Length;

            // Mark previous discharge documents for this invoice as not latest
            var previousDocs = await _context.InvoiceDocuments
                .Where(d => d.InvoiceId == mainInvoice.Id && d.DocumentType == "DischargeSummaryBill")
                .ToListAsync();
            foreach (var doc in previousDocs)
            {
                doc.IsLatest = false;
            }

            var docRecord = new CareSphere.Models.InvoiceDocument
            {
                Id = Guid.NewGuid(),
                TenantId = mainInvoice.TenantId,
                InvoiceId = mainInvoice.Id,
                DocumentType = "DischargeSummaryBill",
                StoragePath = relativePath,
                FileName = fileName,
                GeneratedAt = DateTime.UtcNow,
                FileSizeBytes = fileSizeBytes,
                IsLatest = true
            };

            _context.InvoiceDocuments.Add(docRecord);
            await _context.SaveChangesAsync();

            return relativePath;
        }

        public async Task<List<CareSphere.Models.InvoiceDocument>> GetDocumentsByInvoiceAsync(Guid invoiceId)
        {
            return await _context.InvoiceDocuments
                .Where(d => d.InvoiceId == invoiceId)
                .OrderByDescending(d => d.GeneratedAt)
                .ToListAsync();
        }
    }
}
