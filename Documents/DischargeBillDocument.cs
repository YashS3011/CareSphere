using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CareSphere.Models;

namespace CareSphere.Documents
{
    public class DischargeBillDocument : IDocument
    {
        private readonly Encounter _encounter;
        private readonly List<BillingInvoice> _invoices;
        private readonly List<InsuranceClaim> _claims;

        public DischargeBillDocument(Encounter encounter, List<BillingInvoice> invoices, List<InsuranceClaim> claims)
        {
            _encounter = encounter;
            _invoices = invoices ?? new List<BillingInvoice>();
            _claims = claims ?? new List<InsuranceClaim>();
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
                        column.Item().Text("Discharge Billing Summary").FontSize(12).Bold().FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(200).Column(column =>
                    {
                        column.Item().Text($"Encounter ID: {_encounter.Id.ToString().Substring(0, 8).ToUpper()}").Bold();
                        column.Item().Text($"Date: {DateTime.Now:dd MMM yyyy}");
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Spacing(15);

                    // Patient Stay Information
                    column.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PATIENT DETAILS").Bold().FontSize(9).FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Name: {_encounter.Patient.FirstName} {_encounter.Patient.LastName}");
                            c.Item().Text($"MRN: {_encounter.Patient.Mrn}");
                            c.Item().Text($"Phone: {_encounter.Patient.Phone}");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("ADMISSION DETAILS").Bold().FontSize(9).FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Admission Date: {_encounter.AdmissionDate:dd MMM yyyy HH:mm}");
                            c.Item().Text($"Discharge Date: {(_encounter.DischargeDate.HasValue ? _encounter.DischargeDate.Value.ToString("dd MMM yyyy HH:mm") : "N/A")}");
                            c.Item().Text($"Encounter Type: {_encounter.EncounterType}");
                        });
                    });

                    // Invoices and Line Items grouped by ItemType
                    var allLineItems = _invoices.SelectMany(i => i.BillingLineItems).ToList();
                    var groupedLineItems = allLineItems.GroupBy(item => item.ItemType).ToList();

                    column.Item().Text("CHARGES SUMMARY (GROUPED BY TYPE)").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Category / Description").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Quantity").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Total Amount").Bold();
                        });

                        foreach (var group in groupedLineItems)
                        {
                            // Group Header
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text(group.Key).Bold().FontColor(Colors.Blue.Darken3);
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("");
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text($"INR {group.Sum(x => x.LineTotal):N2}").Bold().FontColor(Colors.Blue.Darken3);

                            foreach (var item in group)
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingLeft(15).Padding(5).Text(item.ItemDescription);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(item.Quantity.ToString("G29"));
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"INR {item.LineTotal:N2}");
                            }
                        }
                    });

                    // Summary Calculations
                    decimal totalCharges = allLineItems.Sum(li => li.LineTotal);
                    decimal totalPayments = _invoices.Sum(i => i.PaidAmount);
                    decimal balanceDue = totalCharges - totalPayments;

                    column.Item().AlignRight().Width(250).Column(c =>
                    {
                        c.Spacing(3);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Stay Charges:");
                            r.ConstantItem(100).AlignRight().Text($"INR {totalCharges:N2}").Bold();
                        });
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Payments Received:");
                            r.ConstantItem(100).AlignRight().Text($"INR {totalPayments:N2}").FontColor(Colors.Green.Darken2);
                        });
                        c.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("Net Balance Due:").Bold().FontColor(Colors.Blue.Darken3);
                            r.ConstantItem(100).AlignRight().Text($"INR {balanceDue:N2}").Bold().FontColor(Colors.Blue.Darken3);
                        });
                    });

                    // Insurance Claim details if any exist
                    if (_claims.Any())
                    {
                        column.Item().PaddingTop(15).Column(c =>
                        {
                            c.Item().Text("INSURANCE CLAIMS").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                            c.Spacing(5);
                            foreach (var claim in _claims)
                            {
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text($"{claim.InsuranceProvider} (Policy: {claim.PolicyNumber}) - Status: {claim.Status}");
                                    r.ConstantItem(150).AlignRight().Text($"Claimed: INR {claim.ClaimedAmount:N2}");
                                });
                                if (claim.ApprovedAmount.HasValue)
                                {
                                    c.Item().Row(r =>
                                    {
                                        r.RelativeItem().PaddingLeft(10).Text("Approved Amount:").Italic();
                                        r.ConstantItem(150).AlignRight().Text($"INR {claim.ApprovedAmount.Value:N2}").Italic();
                                    });
                                }
                            }
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
