using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CareSphere.Models;

namespace CareSphere.Documents
{
    public class LabReportDocument : IDocument
    {
        private readonly LabRequisition _requisition;
        private readonly List<LabResult> _results;

        public LabReportDocument(LabRequisition requisition, List<LabResult> results)
        {
            _requisition = requisition;
            _results = results;
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
                        column.Item().Text("Advanced Diagnostic & Laboratory Services").FontSize(9).Italic();
                        column.Item().PaddingTop(10).Text("Laboratory Test Report").FontSize(14).Bold().FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(200).Column(column =>
                    {
                        column.Item().Text($"Requisition #: {_requisition.RequisitionNumber}").Bold();
                        column.Item().Text($"Order Date: {_requisition.OrderedAt:dd MMM yyyy HH:mm}");
                        column.Item().Text($"Priority: {_requisition.Priority}").Bold().FontColor(
                            _requisition.Priority == "Stat" ? Colors.Red.Medium : 
                            _requisition.Priority == "Urgent" ? Colors.Orange.Medium : Colors.Green.Medium);
                        column.Item().Text($"Status: {_requisition.Status}").Bold().FontColor(
                            _requisition.Status == "Completed" ? Colors.Green.Medium : Colors.Blue.Medium);
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Spacing(15);

                    // Patient & Doctor Info
                    column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PATIENT INFORMATION").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Name: {_requisition.Patient.FirstName} {_requisition.Patient.LastName}");
                            c.Item().Text($"MRN: {_requisition.Patient.Mrn}");
                            c.Item().Text($"DOB / Age: {_requisition.Patient.DateOfBirth:dd MMM yyyy}");
                            c.Item().Text($"Gender: {_requisition.Patient.Gender}");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("ORDERING PROVIDER").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Name: Dr. {_requisition.OrderedByDoctor.FirstName} {_requisition.OrderedByDoctor.LastName}");
                            c.Item().Text($"Reg No: {_requisition.OrderedByDoctor.RegistrationNumber}");
                            c.Item().Text($"Specialization: {_requisition.OrderedByDoctor.Specialization}");
                        });
                    });

                    if (!string.IsNullOrEmpty(_requisition.ClinicalNotes))
                    {
                        column.Item().Column(c =>
                        {
                            c.Item().Text("Clinical Notes:").Bold().FontSize(9);
                            c.Item().Text(_requisition.ClinicalNotes).FontSize(9).Italic();
                        });
                    }

                    // Results Grouped by Test
                    foreach (var item in _requisition.Items)
                    {
                        var testName = item.Test.TestName;
                        var testCode = item.Test.TestCode;
                        var itemResults = _results.Where(r => r.RequisitionItemId == item.Id).ToList();

                        column.Item().Column(testCol =>
                        {
                            testCol.Spacing(5);
                            testCol.Item().Background(Colors.Grey.Lighten3).Padding(5).Text($"{testName} ({testCode})").Bold().FontColor(Colors.Blue.Darken3);

                            testCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Parameter Name
                                    columns.RelativeColumn(1.5f); // Value
                                    columns.RelativeColumn(1); // Unit
                                    columns.RelativeColumn(2.5f); // Reference Range
                                    columns.RelativeColumn(1); // Flag
                                });

                                // Table Header
                                table.Header(header =>
                                {
                                    header.Cell().Padding(3).Text("Parameter").Bold();
                                    header.Cell().Padding(3).Text("Value").Bold();
                                    header.Cell().Padding(3).Text("Unit").Bold();
                                    header.Cell().Padding(3).Text("Reference Range").Bold();
                                    header.Cell().Padding(3).Text("Flag").Bold();
                                });

                                foreach (var res in itemResults)
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(res.Parameter.ParameterName);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(res.ResultValue);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(res.ResultUnit ?? "-");

                                    string refRange = "-";
                                    if (res.ReferenceRangeLow.HasValue || res.ReferenceRangeHigh.HasValue)
                                    {
                                        refRange = $"{res.ReferenceRangeLow} - {res.ReferenceRangeHigh}";
                                    }
                                    else if (!string.IsNullOrEmpty(res.ReferenceRangeText))
                                    {
                                        refRange = res.ReferenceRangeText;
                                    }
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(refRange);

                                    // Flag
                                    var cell = table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3);
                                    if (!string.IsNullOrEmpty(res.AbnormalFlag))
                                    {
                                        var fText = cell.Text(res.AbnormalFlag).Bold();
                                        if (res.AbnormalFlag == "H" || res.AbnormalFlag == "HH")
                                        {
                                            fText.FontColor(Colors.Red.Medium);
                                        }
                                        else if (res.AbnormalFlag == "L" || res.AbnormalFlag == "LL")
                                        {
                                            fText.FontColor(Colors.Blue.Medium);
                                        }
                                        else if (res.AbnormalFlag == "A")
                                        {
                                            fText.FontColor(Colors.Orange.Medium);
                                        }
                                    }
                                    else
                                    {
                                        cell.Text("-");
                                    }
                                }
                            });
                        });
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Spacing(5);
                    col.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(5).Row(row =>
                    {
                        var verifiedByUser = _results.FirstOrDefault()?.VerifiedByUserId ?? "Verified System";
                        row.RelativeItem().Text($"Report Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC");
                        row.RelativeItem().AlignRight().Text($"Verified By: {verifiedByUser}");
                    });
                    col.Item().AlignCenter().Text(t =>
                    {
                        t.Span("Page ");
                        t.CurrentPageNumber();
                        t.Span(" of ");
                        t.TotalPages();
                    });
                });
            });
        }
    }
}
