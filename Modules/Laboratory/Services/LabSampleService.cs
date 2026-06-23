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

namespace CareSphere.Modules.Laboratory.Services
{
    public class LabSampleService : ILabSampleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public LabSampleService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<LabSample> RecordSampleCollectionAsync(Guid tenantId, Guid requisitionId, string sampleType, string collectedByUserId, string? barcodeLabel, string? notes, DateTime collectedAt)
        {
            var requisition = await _context.LabRequisitions
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == requisitionId);

            if (requisition == null)
            {
                throw new KeyNotFoundException("Requisition not found.");
            }

            var sample = new LabSample
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RequisitionId = requisitionId,
                SampleType = sampleType,
                CollectedAt = collectedAt.ToUniversalTime(),
                CollectedByUserId = collectedByUserId,
                BarcodeLabel = barcodeLabel,
                Notes = notes,
                IsReceived = false
            };

            _context.LabSamples.Add(sample);

            requisition.Status = "SampleCollected";
            requisition.UpdatedAt = DateTime.UtcNow;

            foreach (var item in requisition.Items)
            {
                item.Status = "SampleCollected";
            }

            await _context.SaveChangesAsync();

            // Write to AuditEvents
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "SAMPLE_COLLECTED",
                ResourceType = "LabSample",
                ResourceId = sample.Id.ToString(),
                TenantId = tenantId
            });

            return sample;
        }

        public async Task ReceiveSampleAsync(Guid sampleId, string receivedByUserId)
        {
            var sample = await _context.LabSamples
                .Include(s => s.Requisition)
                    .ThenInclude(r => r.Items)
                .FirstOrDefaultAsync(s => s.Id == sampleId);

            if (sample == null)
            {
                throw new KeyNotFoundException("Sample not found.");
            }

            sample.IsReceived = true;
            sample.ReceivedAt = DateTime.UtcNow;
            sample.ReceivedByUserId = receivedByUserId;

            _context.LabSamples.Update(sample);

            var requisition = sample.Requisition;
            requisition.Status = "Processing";
            requisition.UpdatedAt = DateTime.UtcNow;

            foreach (var item in requisition.Items)
            {
                item.Status = "Processing";
            }

            await _context.SaveChangesAsync();

            // Write to AuditEvents
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "SAMPLE_RECEIVED",
                ResourceType = "LabSample",
                ResourceId = sampleId.ToString(),
                TenantId = sample.TenantId
            });
        }

        public async Task<List<LabSample>> GetSamplesByRequisitionAsync(Guid tenantId, Guid requisitionId)
        {
            return await _context.LabSamples.AsNoTracking()
                .Where(s => s.TenantId == tenantId && s.RequisitionId == requisitionId)
                .OrderByDescending(s => s.CollectedAt)
                .ToListAsync();
        }
    }
}
