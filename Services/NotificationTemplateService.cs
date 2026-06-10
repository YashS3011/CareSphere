using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationTemplateService(
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

        public async Task<NotificationTemplate?> GetTemplateAsync(Guid tenantId, string notificationType, string channel, string language)
        {
            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.TenantId == tenantId && 
                                          t.NotificationType == notificationType && 
                                          t.Channel == channel && 
                                          t.Language == language && 
                                          t.IsActive);
            
            if (template != null)
            {
                return template;
            }

            if (language != "en")
            {
                return await _context.NotificationTemplates
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && 
                                              t.NotificationType == notificationType && 
                                              t.Channel == channel && 
                                              t.Language == "en" && 
                                              t.IsActive);
            }

            return null;
        }

        public Task<string> RenderTemplateAsync(string templateBody, Dictionary<string, string> placeholderValues)
        {
            if (string.IsNullOrEmpty(templateBody))
            {
                return Task.FromResult(string.Empty);
            }

            var result = templateBody;
            foreach (var kvp in placeholderValues)
            {
                result = result.Replace("{" + kvp.Key + "}", kvp.Value ?? string.Empty);
            }

            return Task.FromResult(result);
        }

        public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template)
        {
            var exists = await _context.NotificationTemplates.AnyAsync(t => 
                t.TenantId == template.TenantId && 
                t.TemplateName.ToLower() == template.TemplateName.ToLower() && 
                t.Channel == template.Channel && 
                t.Language == template.Language);

            if (exists)
            {
                throw new InvalidOperationException($"A template named '{template.TemplateName}' already exists for this channel and language.");
            }

            template.Id = Guid.NewGuid();
            template.CreatedAt = DateTime.UtcNow;

            _context.NotificationTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Audit with logged-in user ID
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = CurrentUserId,
                Action = "NOTIFICATION_TEMPLATE_CREATED",
                ResourceType = "NotificationTemplate",
                ResourceId = template.Id.ToString(),
                TenantId = template.TenantId
            });

            return template;
        }

        public async Task<NotificationTemplate> UpdateTemplateAsync(Guid id, string templateBody, bool isActive)
        {
            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template == null)
            {
                throw new KeyNotFoundException("Template not found.");
            }

            template.TemplateBody = templateBody;
            template.IsActive = isActive;

            _context.NotificationTemplates.Update(template);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = CurrentUserId,
                Action = "NOTIFICATION_TEMPLATE_UPDATED",
                ResourceType = "NotificationTemplate",
                ResourceId = template.Id.ToString(),
                TenantId = template.TenantId
            });

            return template;
        }

        public async Task<Dictionary<string, List<NotificationTemplate>>> GetAllTemplatesAsync(Guid tenantId)
        {
            var templates = await _context.NotificationTemplates
                .Where(t => t.TenantId == tenantId)
                .OrderBy(t => t.TemplateName)
                .ToListAsync();

            return templates.GroupBy(t => t.NotificationType)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task SeedDefaultTemplatesAsync(Guid tenantId)
        {
            var hasTemplates = await _context.NotificationTemplates.AnyAsync(t => t.TenantId == tenantId);
            if (hasTemplates)
            {
                return;
            }

            var defaults = new List<NotificationTemplate>
            {
                new NotificationTemplate
                {
                    TenantId = tenantId,
                    TemplateName = "Appointment Reminder SMS",
                    NotificationType = "AppointmentReminder",
                    Channel = "SMS",
                    Language = "en",
                    TemplateBody = "Dear {PatientName} this is a reminder for your appointment with Dr {DoctorName} on {AppointmentDate} at {Time} at {HospitalName}. Please arrive 15 minutes early."
                },
                new NotificationTemplate
                {
                    TenantId = tenantId,
                    TemplateName = "Appointment Reminder WhatsApp",
                    NotificationType = "AppointmentReminder",
                    Channel = "WhatsApp",
                    Language = "en",
                    TemplateBody = "Dear {PatientName} this is a reminder for your appointment with Dr {DoctorName} on {AppointmentDate} at {Time} at {HospitalName}. Please arrive 15 minutes early."
                },
                new NotificationTemplate
                {
                    TenantId = tenantId,
                    TemplateName = "Appointment Reminder InApp",
                    NotificationType = "AppointmentReminder",
                    Channel = "InApp",
                    Language = "en",
                    TemplateBody = "Your appointment with Dr {DoctorName} is scheduled on {AppointmentDate} at {Time}."
                },
                new NotificationTemplate
                {
                    TenantId = tenantId,
                    TemplateName = "FollowUp Reminder SMS",
                    NotificationType = "FollowUpReminder",
                    Channel = "SMS",
                    Language = "en",
                    TemplateBody = "Dear {PatientName} Dr {DoctorName} has recommended a follow-up visit. Please contact {HospitalName} to schedule your appointment."
                },
                new NotificationTemplate
                {
                    TenantId = tenantId,
                    TemplateName = "Discharge Notification SMS",
                    NotificationType = "DischargeNotification",
                    Channel = "SMS",
                    Language = "en",
                    TemplateBody = "Dear {PatientName} you have been successfully discharged from {HospitalName} on {DischargeDate}. Thank you for choosing us. Please follow your discharge instructions."
                },
                new NotificationTemplate
                {
                    TenantId = tenantId,
                    TemplateName = "Lab Report Ready SMS",
                    NotificationType = "LabReportReady",
                    Channel = "SMS",
                    Language = "en",
                    TemplateBody = "Dear {PatientName} your lab report {RequisitionNumber} is ready at {HospitalName}. Please contact your doctor for details."
                },
                new NotificationTemplate
                {
                    TenantId = tenantId,
                    TemplateName = "Lab Report Ready InApp",
                    NotificationType = "LabReportReady",
                    Channel = "InApp",
                    Language = "en",
                    TemplateBody = "Your lab report {RequisitionNumber} is now available. Please review with your doctor."
                }
            };

            foreach (var template in defaults)
            {
                template.Id = Guid.NewGuid();
                template.CreatedAt = DateTime.UtcNow;
                _context.NotificationTemplates.Add(template);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = CurrentUserId,
                Action = "NOTIFICATION_TEMPLATES_SEEDED",
                ResourceType = "NotificationTemplate",
                ResourceId = tenantId.ToString(),
                TenantId = tenantId
            });
        }
    }
}

