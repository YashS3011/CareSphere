using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class PatientPreferenceService : IPatientPreferenceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PatientPreferenceService(
            ApplicationDbContext context,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string CurrentUserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

        public async Task<PatientPreference> GetOrCreatePreferencesAsync(Guid tenantId, Guid patientId)
        {
            var pref = await _context.PatientPreferences
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (pref == null)
            {
                pref = new PatientPreference
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PatientId = patientId,
                    PreferredLanguage = "en",
                    PreferredChannel = "SMS",
                    OptOutSms = false,
                    OptOutWhatsApp = false,
                    OptOutVoice = false,
                    AllowAppointmentReminders = true,
                    AllowFollowUpReminders = true,
                    AllowDischargeNotifications = true,
                    AllowLabNotifications = true,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PatientPreferences.Add(pref);
                await _context.SaveChangesAsync();

                // Audit with logged-in user ID
                await _auditService.LogAsync(new AuditEvent
                {
                    UserId = CurrentUserId,
                    Action = "PATIENT_PREFERENCES_CREATED",
                    ResourceType = "PatientPreference",
                    ResourceId = pref.Id.ToString(),
                    TenantId = tenantId
                });
            }

            return pref;
        }

        public async Task<PatientPreference> UpdatePreferencesAsync(PatientPreference preferences)
        {
            var existing = await _context.PatientPreferences.FindAsync(preferences.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"PatientPreference {preferences.Id} not found.");
            }

            existing.PreferredLanguage = preferences.PreferredLanguage;
            existing.PreferredChannel = preferences.PreferredChannel;
            existing.OptOutSms = preferences.OptOutSms;
            existing.OptOutWhatsApp = preferences.OptOutWhatsApp;
            existing.OptOutVoice = preferences.OptOutVoice;
            existing.AllowAppointmentReminders = preferences.AllowAppointmentReminders;
            existing.AllowFollowUpReminders = preferences.AllowFollowUpReminders;
            existing.AllowDischargeNotifications = preferences.AllowDischargeNotifications;
            existing.AllowLabNotifications = preferences.AllowLabNotifications;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.PatientPreferences.Update(existing);
            await _context.SaveChangesAsync();

            // Audit with logged-in user ID
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = CurrentUserId,
                Action = "PATIENT_PREFERENCES_UPDATED",
                ResourceType = "PatientPreference",
                ResourceId = existing.Id.ToString(),
                TenantId = existing.TenantId
            });

            return existing;
        }
    }
}
