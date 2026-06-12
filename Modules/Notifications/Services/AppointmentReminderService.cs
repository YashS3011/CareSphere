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
    public class AppointmentReminderService : IAppointmentReminderService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationTemplateService _templateService;
        private readonly INotificationSenderService _senderService;
        private readonly IServiceBusService _serviceBusService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AppointmentReminderService> _logger;

        public AppointmentReminderService(
            ApplicationDbContext context,
            INotificationTemplateService templateService,
            INotificationSenderService senderService,
            IServiceBusService serviceBusService,
            IAuditService auditService,
            IConfiguration configuration,
            ILogger<AppointmentReminderService> logger)
        {
            _context = context;
            _templateService = templateService;
            _senderService = senderService;
            _serviceBusService = serviceBusService;
            _auditService = auditService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ScheduleAppointmentReminderAsync(Guid tenantId, Guid patientId, Guid? doctorId, DateTime appointmentDate, Guid? appointmentId = null, string language = "en")
        {
            var pref = await _context.PatientPreferences.FirstOrDefaultAsync(p => p.PatientId == patientId);
            var channel = pref?.PreferredChannel ?? "SMS";
            var prefLang = pref?.PreferredLanguage ?? language;

            // TwentyFourHour reminder
            var reminder24h = new AppointmentReminder
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AppointmentId = appointmentId,
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = appointmentDate,
                ReminderType = "TwentyFourHour",
                Channel = channel,
                Language = prefLang,
                Status = "Scheduled",
                ScheduledAt = appointmentDate.AddHours(-24)
            };

            // TwoHour reminder
            var reminder2h = new AppointmentReminder
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AppointmentId = appointmentId,
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = appointmentDate,
                ReminderType = "TwoHour",
                Channel = channel,
                Language = prefLang,
                Status = "Scheduled",
                ScheduledAt = appointmentDate.AddHours(-2)
            };

            _context.AppointmentReminders.Add(reminder24h);
            _context.AppointmentReminders.Add(reminder2h);
            await _context.SaveChangesAsync();

            // Enqueue messages
            await _serviceBusService.EnqueueMessageAsync("AppointmentReminder", new { ReminderId = reminder24h.Id }, tenantId, reminder24h.ScheduledAt);
            await _serviceBusService.EnqueueMessageAsync("AppointmentReminder", new { ReminderId = reminder2h.Id }, tenantId, reminder2h.ScheduledAt);

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "APPOINTMENT_REMINDERS_SCHEDULED",
                ResourceType = "Patient",
                ResourceId = patientId.ToString(),
                TenantId = tenantId
            });
        }

        public async Task ScheduleFollowUpReminderAsync(Guid tenantId, Guid patientId, Guid? doctorId, DateTime followUpDate, string language = "en")
        {
            var pref = await _context.PatientPreferences.FirstOrDefaultAsync(p => p.PatientId == patientId);
            var channel = pref?.PreferredChannel ?? "SMS";
            var prefLang = pref?.PreferredLanguage ?? language;

            var reminder = new AppointmentReminder
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AppointmentId = null,
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = followUpDate,
                ReminderType = "FollowUp",
                Channel = channel,
                Language = prefLang,
                Status = "Scheduled",
                ScheduledAt = followUpDate.AddHours(-24)
            };

            _context.AppointmentReminders.Add(reminder);
            await _context.SaveChangesAsync();

            await _serviceBusService.EnqueueMessageAsync("AppointmentReminder", new { ReminderId = reminder.Id }, tenantId, reminder.ScheduledAt);

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "FOLLOW_UP_REMINDER_SCHEDULED",
                ResourceType = "Patient",
                ResourceId = patientId.ToString(),
                TenantId = tenantId
            });
        }

        public async Task ProcessAppointmentReminderAsync(Guid reminderId)
        {
            var reminder = await _context.AppointmentReminders
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r => r.Id == reminderId);

            if (reminder == null)
            {
                _logger.LogWarning($"Appointment reminder {reminderId} not found.");
                return;
            }

            if (reminder.Status != "Scheduled")
            {
                _logger.LogInformation($"Reminder {reminderId} is not in 'Scheduled' status (Status: {reminder.Status}). Skipping processing.");
                return;
            }

            // Load doctor
            Doctor? doctor = null;
            if (reminder.DoctorId.HasValue)
            {
                doctor = await _context.Doctors.FindAsync(reminder.DoctorId.Value);
            }

            var notificationType = reminder.ReminderType == "FollowUp" ? "FollowUpReminder" : "AppointmentReminder";

            // Load template
            var template = await _templateService.GetTemplateAsync(reminder.TenantId, notificationType, reminder.Channel, reminder.Language);
            var templateBody = template?.TemplateBody;

            if (string.IsNullOrEmpty(templateBody))
            {
                _logger.LogWarning($"No template body found for Tenant: {reminder.TenantId}, Type: {notificationType}, Channel: {reminder.Channel}, Language: {reminder.Language}");
                return;
            }

            var patientName = $"{reminder.Patient.FirstName} {reminder.Patient.LastName}";
            var doctorName = doctor != null ? $"{doctor.FirstName} {doctor.LastName}" : "Medical Staff";
            var hospitalName = _configuration["HospitalName"] ?? "CareSphere Hospital";

            var placeholders = new Dictionary<string, string>
            {
                { "PatientName", patientName },
                { "DoctorName", doctorName },
                { "AppointmentDate", reminder.AppointmentDate.ToString("dd MMM yyyy") },
                { "Time", reminder.AppointmentDate.ToString("hh:mm tt") },
                { "HospitalName", hospitalName }
            };

            var renderedBody = await _templateService.RenderTemplateAsync(templateBody, placeholders);

            // Send notification
            if (reminder.Channel.Equals("SMS", StringComparison.OrdinalIgnoreCase))
            {
                await _senderService.SendSmsAsync(reminder.TenantId, reminder.Patient.Phone, renderedBody, notificationType, reminder.PatientId, reminder.DoctorId, reminder.Language, reminder.ScheduledAt);
            }
            else if (reminder.Channel.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
            {
                await _senderService.SendWhatsAppAsync(reminder.TenantId, reminder.Patient.Phone, renderedBody, notificationType, reminder.PatientId, reminder.DoctorId, reminder.Language, reminder.ScheduledAt);
            }
            else if (reminder.Channel.Equals("Voice", StringComparison.OrdinalIgnoreCase))
            {
                await _senderService.SendVoiceReminderAsync(reminder.TenantId, reminder.Patient.Phone, renderedBody, reminder.Language, reminder.PatientId, notificationType);
            }
            else if (reminder.Channel.Equals("InApp", StringComparison.OrdinalIgnoreCase))
            {
                await _senderService.SendInAppAsync(reminder.TenantId, "Patient", reminder.PatientId.ToString(), "Appointment Reminder", renderedBody, "Appointment", reminder.AppointmentId?.ToString() ?? Guid.Empty.ToString(), reminder.PatientId, notificationType);
            }

            // Find matching log to link
            var log = await _context.NotificationLogs
                .Where(l => l.TenantId == reminder.TenantId && l.PatientId == reminder.PatientId && l.MessageBody == renderedBody)
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();

            reminder.Status = "Sent";
            reminder.SentAt = DateTime.UtcNow;
            if (log != null)
            {
                reminder.NotificationLogId = log.Id;
            }

            _context.AppointmentReminders.Update(reminder);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "APPOINTMENT_REMINDER_SENT",
                ResourceType = "AppointmentReminder",
                ResourceId = reminder.Id.ToString(),
                TenantId = reminder.TenantId
            });
        }

        public async Task CancelReminderAsync(Guid appointmentId)
        {
            var reminders = await _context.AppointmentReminders
                .Where(r => r.AppointmentId == appointmentId && r.Status == "Scheduled")
                .ToListAsync();

            if (!reminders.Any())
            {
                return;
            }

            foreach (var reminder in reminders)
            {
                reminder.Status = "Cancelled";
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "APPOINTMENT_REMINDER_CANCELLED",
                ResourceType = "Appointment",
                ResourceId = appointmentId.ToString(),
                TenantId = reminders.First().TenantId
            });
        }

        public async Task<List<AppointmentReminder>> GetScheduledRemindersAsync(Guid tenantId)
        {
            return await _context.AppointmentReminders
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => r.TenantId == tenantId && r.Status == "Scheduled")
                .OrderBy(r => r.ScheduledAt)
                .ToListAsync();
        }

        public async Task ScheduleReminderForAppointmentAsync(Guid tenantId, Guid patientId, Guid? doctorId, DateTime appointmentDate, Guid? appointmentId = null, string language = "en")
        {
            // Call main schedule implementation
            await ScheduleAppointmentReminderAsync(tenantId, patientId, doctorId, appointmentDate, appointmentId, language);
        }
    }
}

