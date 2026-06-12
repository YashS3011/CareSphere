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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CareSphere.Modules.Notifications.Services
{
    public class DischargeNotificationService : IDischargeNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationTemplateService _templateService;
        private readonly INotificationSenderService _senderService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DischargeNotificationService> _logger;

        public DischargeNotificationService(
            ApplicationDbContext context,
            INotificationTemplateService templateService,
            INotificationSenderService senderService,
            IAuditService auditService,
            IConfiguration configuration,
            ILogger<DischargeNotificationService> _logger)
        {
            _context = context;
            _templateService = templateService;
            _senderService = senderService;
            _auditService = auditService;
            _configuration = configuration;
            this._logger = _logger;
        }

        public async Task SendDischargeNotificationAsync(Guid tenantId, Guid patientId, Guid? allotmentId, DateTime dischargedAt, string language = "en")
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                throw new KeyNotFoundException($"Patient {patientId} not found.");
            }

            var pref = await _context.PatientPreferences.FirstOrDefaultAsync(p => p.PatientId == patientId);
            var prefLang = pref?.PreferredLanguage ?? language;
            var preferredChannel = pref?.PreferredChannel ?? "SMS";

            // Load SMS Template
            var template = await _templateService.GetTemplateAsync(tenantId, "DischargeNotification", "SMS", prefLang);
            var templateBody = template?.TemplateBody;

            if (string.IsNullOrEmpty(templateBody))
            {
                // Simple hardcoded fallback if template is completely missing
                templateBody = "Dear {PatientName} you have been successfully discharged from {HospitalName} on {DischargeDate}. Thank you for choosing us.";
            }

            var patientName = $"{patient.FirstName} {patient.LastName}";
            var hospitalName = _configuration["HospitalName"] ?? "CareSphere Hospital";

            var placeholders = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "DischargeDate", dischargedAt.ToString("dd MMM yyyy") },
                { "HospitalName", hospitalName }
            };

            var renderedBody = await _templateService.RenderTemplateAsync(templateBody, placeholders);

            // Send SMS
            await _senderService.SendSmsAsync(tenantId, patient.Phone, renderedBody, "DischargeNotification", patientId, null, prefLang);

            // Find matching SMS log to link
            var smsLog = await _context.NotificationLogs
                .Where(l => l.TenantId == tenantId && l.PatientId == patientId && l.Channel == "SMS" && l.MessageBody == renderedBody)
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();

            // If WhatsApp is preferred, also send WhatsApp
            if (preferredChannel.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
            {
                await _senderService.SendWhatsAppAsync(tenantId, patient.Phone, renderedBody, "DischargeNotification", patientId, null, prefLang);
            }

            // Always send In-App notification
            var inAppTitle = "Discharged from Hospital";
            await _senderService.SendInAppAsync(
                tenantId,
                recipientType: "Patient",
                recipientId: patientId.ToString(),
                title: inAppTitle,
                messageBody: renderedBody,
                resourceType: "BedAllotment",
                resourceId: allotmentId?.ToString() ?? Guid.Empty.ToString(),
                patientId: patientId,
                notificationType: "DischargeNotification"
            );

            // Record the discharge notification entry
            var dischargeNotif = new DischargeNotification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AllotmentId = allotmentId,
                PatientId = patientId,
                DischargedAt = dischargedAt,
                Channel = preferredChannel,
                Language = prefLang,
                Status = "Sent",
                NotificationLogId = smsLog?.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.DischargeNotifications.Add(dischargeNotif);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "DISCHARGE_NOTIFICATION_SENT",
                ResourceType = "DischargeNotification",
                ResourceId = dischargeNotif.Id.ToString(),
                TenantId = tenantId
            });
        }

        public async Task<List<DischargeNotification>> GetDischargeNotificationsByPatientAsync(Guid patientId)
        {
            return await _context.DischargeNotifications
                .Include(d => d.BedAllotment)
                .Include(d => d.NotificationLog)
                .Where(d => d.PatientId == patientId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
    }
}

