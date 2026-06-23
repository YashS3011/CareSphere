using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;
using CareSphere.Modules.Shared.ReadModels;

namespace CareSphere.Modules.Nursing.Services
{
    public class VitalSignsService : IVitalSignsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPatientReadModel _patientReadModel;
        private readonly IVitalThresholdService _thresholdService;

        public VitalSignsService(ApplicationDbContext dbContext, IPatientReadModel patientReadModel, IVitalThresholdService thresholdService)
        {
            _dbContext = dbContext;
            _patientReadModel = patientReadModel;
            _thresholdService = thresholdService;
        }

        public async Task<VitalSigns> RecordAsync(VitalSigns vitals, Guid tenantId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                vitals.Id = Guid.NewGuid();
                vitals.TenantId = tenantId;
                if (vitals.RecordedAt == default)
                {
                    vitals.RecordedAt = DateTime.UtcNow;
                }
                vitals.CreatedAt = DateTime.UtcNow;

                _dbContext.VitalSigns.Add(vitals);

                // Check critical alert thresholds
                var breaches = _thresholdService.CheckThresholds(vitals);

                if (breaches.Any())
                {
                    // Load active bed allotment to find admitting doctor
                    var allotment = await _dbContext.BedAllotments
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == vitals.BedAllotmentId && a.TenantId == tenantId);

                    Guid? doctorId = null;
                    if (allotment != null && !string.IsNullOrEmpty(allotment.AdmittingDoctor))
                    {
                        var docName = allotment.AdmittingDoctor.Replace("Dr.", "").Trim().ToLower();
                        var matchedDoctor = await _dbContext.Doctors
                            .AsNoTracking()
                            .Where(d => d.TenantId == tenantId && d.IsActive)
                            .FirstOrDefaultAsync(d => d.LastName.ToLower().Contains(docName) || 
                                                      d.FirstName.ToLower().Contains(docName) || 
                                                      (d.FirstName + " " + d.LastName).ToLower().Contains(docName));

                        if (matchedDoctor != null)
                        {
                            doctorId = matchedDoctor.Id;
                        }
                    }

                    if (doctorId == null)
                    {
                        // Fallback to the first active doctor in tenant
                        var fallbackDoctor = await _dbContext.Doctors
                            .AsNoTracking()
                            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.IsActive);
                        doctorId = fallbackDoctor?.Id;
                    }

                    var patientSummary = await _patientReadModel.GetSummaryAsync(vitals.PatientId, tenantId);
                    var mrn = patientSummary?.MRN ?? "Unknown";

                    var notification = new InAppNotification
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        RecipientType = "Doctor",
                        RecipientId = doctorId?.ToString() ?? string.Empty,
                        Title = "⚠️ Critical Vitals Alert",
                        Message = $"Patient {mrn} has critical vitals: {string.Join(", ", breaches)}. Recorded by nurse at {vitals.RecordedAt:yyyy-MM-dd HH:mm}.",
                        ResourceType = "VitalSigns",
                        ResourceId = vitals.Id.ToString(),
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _dbContext.InAppNotifications.Add(notification);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return vitals;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<VitalSigns>> GetHistoryAsync(Guid patientId, Guid tenantId, int lastN = 10)
        {
            return await _dbContext.VitalSigns.AsNoTracking()
                .Where(v => v.PatientId == patientId && v.TenantId == tenantId)
                .OrderByDescending(v => v.RecordedAt)
                .Take(lastN)
                .ToListAsync();
        }

        public async Task<VitalSigns?> GetLatestAsync(Guid patientId, Guid tenantId)
        {
            return await _dbContext.VitalSigns.AsNoTracking()
                .Where(v => v.PatientId == patientId && v.TenantId == tenantId)
                .OrderByDescending(v => v.RecordedAt)
                .FirstOrDefaultAsync();
        }
    }
}
