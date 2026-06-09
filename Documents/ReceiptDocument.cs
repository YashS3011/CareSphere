using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CareSphere.Models;

namespace CareSphere.Documents
{
    public class ReceiptDocument : IDocument
    {
        private readonly Payment _payment;

        public ReceiptDocument(Payment payment)
        {
            _payment = payment;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("CARESPHERE HOSPITAL").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                        column.Item().Text("Payment Receipt").FontSize(11).Bold().FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(150).Column(column =>
                    {
                        column.Item().Text($"Receipt ID: {_payment.Id.ToString().Substring(0, 8).ToUpper()}").Bold();
                        column.Item().Text($"Date: {_payment.PaymentDate:dd MMM yyyy HH:mm}");
                    });
                });

                page.Content().PaddingVertical(0.5f, Unit.Centimetre).Column(column =>
                {
                    column.Spacing(8);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Padding(3).Text("Patient Name:").Bold();
                        table.Cell().Padding(3).Text($"{_payment.Patient.FirstName} {_payment.Patient.LastName}");

                        table.Cell().Padding(3).Text("MRN (Patient ID):").Bold();
                        table.Cell().Padding(3).Text(_payment.Patient.Mrn);

                        table.Cell().Padding(3).Text("Invoice Reference:").Bold();
                        table.Cell().Padding(3).Text(_payment.BillingInvoice.InvoiceNumber);

                        table.Cell().Padding(3).Text("Payment Method:").Bold();
                        table.Cell().Padding(3).Text(_payment.PaymentMethod);

                        if (!string.IsNullOrEmpty(_payment.TransactionReference))
                        {
                            table.Cell().Padding(3).Text("Transaction Reference:").Bold();
                            table.Cell().Padding(3).Text(_payment.TransactionReference);
                        }

                        if (!string.IsNullOrEmpty(_payment.RazorpayPaymentId))
                        {
                            table.Cell().Padding(3).Text("Razorpay Payment ID:").Bold();
                            table.Cell().Padding(3).Text(_payment.RazorpayPaymentId);
                        }
                    });

                    column.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Text("Amount Received:").FontSize(12).Bold().FontColor(Colors.Green.Darken3);
                        row.ConstantItem(150).AlignRight().Text($"INR {_payment.Amount:N2}").FontSize(12).Bold().FontColor(Colors.Green.Darken3);
                    });

                    if (!string.IsNullOrEmpty(_payment.Notes))
                    {
                        column.Item().Text(t =>
                        {
                            t.Span("Notes: ").Bold();
                            t.Span(_payment.Notes).Italic();
                        });
                    }
                });

                page.Footer().BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).AlignLeft().Text(t =>
                {
                    t.Span("Computer generated receipt. Signature not required. Registered user ID: ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span(_payment.RecordedByUserId).FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }
    }
}
