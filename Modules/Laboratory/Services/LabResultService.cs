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
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CareSphere.Modules.Laboratory.Services
{
    public class LabResultService : ILabResultService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILabReportService _reportService;

        public LabResultService(ApplicationDbContext context, IAuditService auditService, ILabReportService reportService)
        {
            _context = context;
            _auditService = auditService;
            _reportService = reportService;
        }

        public async Task<LabResult> EnterResultAsync(Guid tenantId, Guid requisitionItemId, Guid parameterId, string resultValue, decimal? resultNumeric, string? notes, string enteredByUserId)
        {
            var parameter = await _context.LabTestParameters.FindAsync(parameterId);
            if (parameter == null)
            {
                throw new KeyNotFoundException("Parameter not found.");
            }

            var requisitionItem = await _context.LabRequisitionItems
                .Include(ri => ri.Requisition)
                .FirstOrDefaultAsync(ri => ri.Id == requisitionItemId);
            if (requisitionItem == null)
            {
                throw new KeyNotFoundException("Requisition item not found.");
            }

            // Abnormal flagging rules
            bool isAbnormal = false;
            string? flag = null;

            if (parameter.DataType == "Numeric" && resultNumeric.HasValue)
            {
                var val = resultNumeric.Value;
                if (parameter.ReferenceRangeHigh.HasValue && val > parameter.ReferenceRangeHigh.Value)
                {
                    isAbnormal = true;
                    flag = "H";
                    if (val > parameter.ReferenceRangeHigh.Value * 1.5m)
                    {
                        flag = "HH";
                    }
                }
                else if (parameter.ReferenceRangeLow.HasValue && val < parameter.ReferenceRangeLow.Value)
                {
                    isAbnormal = true;
                    flag = "L";
                    if (val < parameter.ReferenceRangeLow.Value * 0.5m)
                    {
                        flag = "LL";
                    }
                }
            }
            else if (parameter.DataType == "Text" && !string.IsNullOrWhiteSpace(parameter.ReferenceRangeText))
            {
                if (!string.Equals(resultValue.Trim(), parameter.ReferenceRangeText.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    isAbnormal = true;
                    flag = "A";
                }
            }
            else if (parameter.DataType == "Boolean" && !string.IsNullOrWhiteSpace(parameter.ReferenceRangeText))
            {
                if (string.Equals(resultValue.Trim(), "Positive", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(parameter.ReferenceRangeText.Trim(), "Negative", StringComparison.OrdinalIgnoreCase))
                {
                    isAbnormal = true;
                    flag = "A";
                }
            }

            // Build FHIR R4 Observation JSON
            var interpretationCoding = flag != null ? new[]
            {
                new
                {
                    system = "http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation",
                    code = flag,
                    display = flag switch
                    {
                        "H" => "High",
                        "HH" => "Critically High",
                        "L" => "Low",
                        "LL" => "Critically Low",
                        "A" => "Abnormal",
                        _ => "Normal"
                    }
                }
            } : null;

            var valueQuantity = (parameter.DataType == "Numeric" && resultNumeric.HasValue) ? new
            {
                value = resultNumeric.Value,
                unit = parameter.Unit,
                system = "http://unitsofmeasure.org"
            } : null;

            var valueString = (parameter.DataType != "Numeric") ? resultValue : null;

            var fhirRanges = (parameter.ReferenceRangeLow.HasValue || parameter.ReferenceRangeHigh.HasValue) ? new[]
            {
                new
                {
                    low = parameter.ReferenceRangeLow.HasValue ? new { value = parameter.ReferenceRangeLow.Value, unit = parameter.Unit } : null,
                    high = parameter.ReferenceRangeHigh.HasValue ? new { value = parameter.ReferenceRangeHigh.Value, unit = parameter.Unit } : null
                }
            } : null;

            var fhirObservation = new
            {
                resourceType = "Observation",
                status = "preliminary",
                code = new
                {
                    coding = new[]
                    {
                        new { code = parameter.ParameterCode, display = parameter.ParameterName, system = "http://loinc.org" }
                    },
                    text = parameter.ParameterName
                },
                subject = new { reference = $"Patient/{requisitionItem.Requisition.PatientId}" },
                encounter = requisitionItem.Requisition.EncounterId.HasValue ? new { reference = $"Encounter/{requisitionItem.Requisition.EncounterId}" } : null,
                valueQuantity = valueQuantity,
                valueString = valueString,
                interpretation = interpretationCoding != null ? new { coding = interpretationCoding } : null,
                referenceRange = fhirRanges,
                effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            // Sync FhirObservationJson to HAPI FHIR when server is available
            var fhirObservationJson = JsonSerializer.Serialize(fhirObservation);

            var result = await _context.LabResults
                .FirstOrDefaultAsync(r => r.TenantId == tenantId &&
                                          r.RequisitionItemId == requisitionItemId &&
                                          r.ParameterId == parameterId);

            if (result == null)
            {
                result = new LabResult
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RequisitionItemId = requisitionItemId,
                    ParameterId = parameterId
                };
                _context.LabResults.Add(result);
            }

            result.ResultValue = resultValue;
            result.ResultNumeric = resultNumeric;
            result.ResultUnit = parameter.Unit;
            result.ReferenceRangeLow = parameter.ReferenceRangeLow;
            result.ReferenceRangeHigh = parameter.ReferenceRangeHigh;
            result.ReferenceRangeText = parameter.ReferenceRangeText;
            result.IsAbnormal = isAbnormal;
            result.AbnormalFlag = flag;
            result.EnteredByUserId = enteredByUserId;
            result.EnteredAt = DateTime.UtcNow;
            result.Notes = notes;
            result.FhirObservationJson = fhirObservationJson;

            await _context.SaveChangesAsync();

            // Write to AuditEvents
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "LAB_RESULT_ENTERED",
                ResourceType = "LabResult",
                ResourceId = result.Id.ToString(),
                TenantId = tenantId
            });

            return result;
        }

        public async Task VerifyResultAsync(Guid resultId, string verifiedByUserId)
        {
            var result = await _context.LabResults
                .Include(r => r.RequisitionItem)
                    .ThenInclude(ri => ri.Requisition)
                        .ThenInclude(r => r.Items)
                .Include(r => r.Parameter)
                .FirstOrDefaultAsync(r => r.Id == resultId);

            if (result == null)
            {
                throw new KeyNotFoundException("Result not found.");
            }

            result.VerifiedByUserId = verifiedByUserId;
            result.VerifiedAt = DateTime.UtcNow;
            _context.LabResults.Update(result);
            await _context.SaveChangesAsync();

            // Write to AuditEvents
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "LAB_RESULT_VERIFIED",
                ResourceType = "LabResult",
                ResourceId = resultId.ToString(),
                TenantId = result.TenantId
            });

            var requisitionItem = result.RequisitionItem;

            // Check if all parameters for the requisition item have a result entered and verified.
            // First load parameters of this test.
            var testParameters = await _context.LabTestParameters
                .Where(p => p.TestId == requisitionItem.TestId)
                .ToListAsync();

            var itemResults = await _context.LabResults
                .Where(r => r.RequisitionItemId == requisitionItem.Id)
                .ToListAsync();

            var allParametersVerified = testParameters.All(p =>
                itemResults.Any(r => r.ParameterId == p.Id && r.VerifiedAt.HasValue));

            if (allParametersVerified)
            {
                requisitionItem.Status = "Completed";
                _context.LabRequisitionItems.Update(requisitionItem);
                await _context.SaveChangesAsync();

                var requisition = requisitionItem.Requisition;
                var allItemsCompleted = requisition.Items.All(i => i.Status == "Completed");

                if (allItemsCompleted)
                {
                    requisition.Status = "Completed";
                    requisition.UpdatedAt = DateTime.UtcNow;
                    _context.LabRequisitions.Update(requisition);
                    await _context.SaveChangesAsync();

                    // Trigger report generation
                    await _reportService.GenerateReportAsync(requisition.TenantId, requisition.Id);
                }
            }
        }

        public async Task<List<LabResult>> GetResultsByRequisitionItemAsync(Guid tenantId, Guid requisitionItemId)
        {
            return await _context.LabResults
                .Include(r => r.Parameter)
                .Where(r => r.TenantId == tenantId && r.RequisitionItemId == requisitionItemId)
                .OrderBy(r => r.Parameter.SortOrder)
                .ToListAsync();
        }

        public async Task<List<LabResult>> GetAbnormalResultsByPatientAsync(Guid tenantId, Guid patientId)
        {
            return await _context.LabResults
                .Include(r => r.Parameter)
                .Include(r => r.RequisitionItem)
                    .ThenInclude(ri => ri.Requisition)
                .Include(r => r.RequisitionItem)
                    .ThenInclude(ri => ri.Test)
                .Where(r => r.TenantId == tenantId && r.IsAbnormal && r.RequisitionItem.Requisition.PatientId == patientId)
                .OrderByDescending(r => r.EnteredAt)
                .ToListAsync();
        }
    }
}
