using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CareSphere.Models;

namespace CareSphere.Documents
{
    public class InvoiceDocument : IDocument
    {
        private readonly BillingInvoice _invoice;

        public InvoiceDocument(BillingInvoice invoice)
        {
            _invoice = invoice;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("CARESPHERE HOSPITAL").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        column.Item().Text("Advanced Healthcare Services").FontSize(9).Italic();
                        column.Item().PaddingTop(10).Text("Patient Bill & Invoice").FontSize(14).Bold().FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(200).Column(column =>
                    {
                        column.Item().Text($"Invoice Number: {_invoice.InvoiceNumber}").Bold();
                        column.Item().Text($"Invoice Date: {_invoice.InvoiceDate:dd MMM yyyy}");
                        if (_invoice.DueDate.HasValue)
                        {
                            column.Item().Text($"Due Date: {_invoice.DueDate.Value:dd MMM yyyy}").FontColor(Colors.Red.Medium);
                        }
                        column.Item().Text($"Status: {_invoice.Status}").Bold().FontColor(_invoice.Status == "Paid" ? Colors.Green.Medium : Colors.Orange.Medium);
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Spacing(15);

                    // Patient & Encounter Info
                    column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PATIENT INFORMATION").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Name: {_invoice.Patient.FirstName} {_invoice.Patient.LastName}");
                            c.Item().Text($"MRN: {_invoice.Patient.Mrn}");
                            c.Item().Text($"Phone: {_invoice.Patient.Phone}");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("ENCOUNTER DETAILS").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                            if (_invoice.Encounter != null)
                            {
                                c.Item().Text($"Type: {_invoice.Encounter.EncounterType}");
                                c.Item().Text($"Admission Date: {_invoice.Encounter.AdmissionDate:dd MMM yyyy}");
                                if (_invoice.Encounter.DischargeDate.HasValue)
                                {
                                    c.Item().Text($"Discharge Date: {_invoice.Encounter.DischargeDate.Value:dd MMM yyyy}");
                                }
                            }
                            else
                            {
                                c.Item().Text("Type: Outpatient (Walk-in)");
                            }
                        });
                    });

                    // Line Items Table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.2f);
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("#").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Item Details").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Type").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Qty").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Unit Price").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Disc %").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Total").Bold();
                        });

                        int seq = 1;
                        foreach (var item in _invoice.BillingLineItems)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(seq.ToString());
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.ItemDescription);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.ItemType);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(item.Quantity.ToString("G29"));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"INR {item.UnitPrice:N2}");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{item.DiscountPercent:G29}%");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"INR {item.LineTotal:N2}");
                            seq++;
                        }
                    });

                    // Totals
                    column.Item().AlignRight().Width(250).Column(c =>
                    {
                        c.Spacing(3);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:");
                            r.ConstantItem(100).AlignRight().Text($"INR {_invoice.SubtotalAmount:N2}");
                        });
                        if (_invoice.DiscountAmount > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Discount:");
                                r.ConstantItem(100).AlignRight().Text($"- INR {_invoice.DiscountAmount:N2}");
                            });
                        }
                        if (_invoice.TaxAmount > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Tax Amount:");
                                r.ConstantItem(100).AlignRight().Text($"+ INR {_invoice.TaxAmount:N2}");
                            });
                        }
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Amount:").Bold();
                            r.ConstantItem(100).AlignRight().Text($"INR {_invoice.TotalAmount:N2}").Bold();
                        });
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Paid Amount:").FontColor(Colors.Grey.Medium);
                            r.ConstantItem(100).AlignRight().Text($"INR {_invoice.PaidAmount:N2}").FontColor(Colors.Grey.Medium);
                        });
                        c.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("Balance Due:").Bold().FontColor(Colors.Blue.Darken3);
                            r.ConstantItem(100).AlignRight().Text($"INR {_invoice.BalanceAmount:N2}").Bold().FontColor(Colors.Blue.Darken3);
                        });
                    });

                    if (!string.IsNullOrEmpty(_invoice.Notes))
                    {
                        column.Item().PaddingTop(20).Column(c =>
                        {
                            c.Item().Text("Notes & Comments:").Bold();
                            c.Item().Text(_invoice.Notes).FontSize(9).Italic();
                        });
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ");
                    t.CurrentPageNumber();
                    t.Span(" of ");
                    t.TotalPages();
                });
            });
        }
    }
}
