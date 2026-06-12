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
    public class LabRequisitionService : ILabRequisitionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public LabRequisitionService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<LabRequisition> CreateRequisitionAsync(Guid tenantId, Guid patientId, Guid? encounterId, Guid doctorId, string priority, string? clinicalNotes, List<Guid> testIds)
        {
            var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var countToday = await _context.LabRequisitions
                .CountAsync(r => r.TenantId == tenantId && r.RequisitionNumber.StartsWith($"LAB-{todayStr}-"));
            var requisitionNumber = $"LAB-{todayStr}-{(countToday + 1):D4}";

            var requisition = new LabRequisition
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RequisitionNumber = requisitionNumber,
                PatientId = patientId,
                EncounterId = encounterId,
                OrderedByDoctorId = doctorId,
                OrderedAt = DateTime.UtcNow,
                Priority = priority,
                Status = "Ordered",
                ClinicalNotes = clinicalNotes,
                CreatedAt = DateTime.UtcNow
            };

            // Load tests to compile the FHIR JSON
            var tests = await _context.LabTestCatalogs
                .Where(t => testIds.Contains(t.Id))
                .ToListAsync();

            // Build FHIR R4 ServiceRequest JSON
            var fhirPriority = priority.ToLower() switch
            {
                "stat" => "stat",
                "urgent" => "urgent",
                _ => "routine"
            };

            var fhirServiceRequest = new
            {
                resourceType = "ServiceRequest",
                status = "active",
                intent = "order",
                priority = fhirPriority,
                subject = new { reference = $"Patient/{patientId}" },
                encounter = encounterId.HasValue ? new { reference = $"Encounter/{encounterId}" } : null,
                requester = new { reference = $"Practitioner/{doctorId}" },
                code = new
                {
                    coding = tests.Select(t => new
                    {
                        code = t.TestCode,
                        display = t.TestName,
                        system = "http://loinc.org"
                    }).ToArray()
                },
                authoredOn = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            // Sync FhirServiceRequestJson to HAPI FHIR when server is available
            requisition.FhirServiceRequestJson = JsonSerializer.Serialize(fhirServiceRequest);

            _context.LabRequisitions.Add(requisition);

            foreach (var testId in testIds)
            {
                var item = new LabRequisitionItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RequisitionId = requisition.Id,
                    TestId = testId,
                    Status = "Ordered"
                };
                _context.LabRequisitionItems.Add(item);
            }

            await _context.SaveChangesAsync();

            // Write to AuditEvents
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "LAB_REQUISITION_CREATED",
                ResourceType = "LabRequisition",
                ResourceId = requisition.Id.ToString(),
                TenantId = tenantId
            });

            return requisition;
        }

        public async Task<LabRequisition?> GetRequisitionByIdAsync(Guid id)
        {
            return await _context.LabRequisitions
                .Include(r => r.Patient)
                .Include(r => r.OrderedByDoctor)
                .Include(r => r.Encounter)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Test)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Results)
                        .ThenInclude(res => res.Parameter)
                .Include(r => r.Samples)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<LabRequisition>> GetRequisitionsByPatientAsync(Guid tenantId, Guid patientId)
        {
            return await _context.LabRequisitions
                .Include(r => r.OrderedByDoctor)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Test)
                .Where(r => r.TenantId == tenantId && r.PatientId == patientId)
                .OrderByDescending(r => r.OrderedAt)
                .ToListAsync();
        }

        public async Task<List<LabRequisition>> GetRequisitionsByEncounterAsync(Guid tenantId, Guid encounterId)
        {
            return await _context.LabRequisitions
                .Include(r => r.OrderedByDoctor)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Test)
                .Where(r => r.TenantId == tenantId && r.EncounterId == encounterId)
                .OrderByDescending(r => r.OrderedAt)
                .ToListAsync();
        }

        public async Task<List<LabRequisition>> GetPendingRequisitionsAsync(Guid tenantId)
        {
            return await _context.LabRequisitions
                .Include(r => r.Patient)
                .Include(r => r.OrderedByDoctor)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Test)
                .Where(r => r.TenantId == tenantId && r.Status != "Completed" && r.Status != "Cancelled")
                .OrderByDescending(r => r.Priority == "Stat" ? 3 : r.Priority == "Urgent" ? 2 : 1)
                .ThenBy(r => r.OrderedAt)
                .ToListAsync();
        }

        public async Task UpdateRequisitionStatusAsync(Guid requisitionId, string status)
        {
            var requisition = await _context.LabRequisitions
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == requisitionId);

            if (requisition == null)
            {
                throw new KeyNotFoundException("Requisition not found.");
            }

            requisition.Status = status;
            requisition.UpdatedAt = DateTime.UtcNow;

            foreach (var item in requisition.Items)
            {
                item.Status = status;
            }

            await _context.SaveChangesAsync();

            // Write to AuditEvents
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "LAB_REQUISITION_STATUS_UPDATED",
                ResourceType = "LabRequisition",
                ResourceId = requisitionId.ToString(),
                TenantId = requisition.TenantId
            });
        }

        public async Task CancelRequisitionAsync(Guid requisitionId)
        {
            var requisition = await _context.LabRequisitions
                .Include(r => r.Items)
                    .ThenInclude(i => i.Results)
                .FirstOrDefaultAsync(r => r.Id == requisitionId);

            if (requisition == null)
            {
                throw new KeyNotFoundException("Requisition not found.");
            }

            // check if any results have been entered yet
            var hasResults = requisition.Items.Any(i => i.Results.Any());
            if (hasResults)
            {
                throw new InvalidOperationException("Cannot cancel requisition because results have already been entered.");
            }

            requisition.Status = "Cancelled";
            requisition.UpdatedAt = DateTime.UtcNow;

            foreach (var item in requisition.Items)
            {
                item.Status = "Cancelled";
            }

            await _context.SaveChangesAsync();

            // Write to AuditEvents
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "LAB_REQUISITION_CANCELLED",
                ResourceType = "LabRequisition",
                ResourceId = requisitionId.ToString(),
                TenantId = requisition.TenantId
            });
        }
    }
}
