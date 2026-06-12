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
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Modules.Notifications.Services
{
    public interface INotificationTemplateService
    {
        Task<NotificationTemplate?> GetTemplateAsync(Guid tenantId, string notificationType, string channel, string language);
        Task<string> RenderTemplateAsync(string templateBody, Dictionary<string, string> placeholderValues);
        Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template);
        Task<NotificationTemplate> UpdateTemplateAsync(Guid id, string templateBody, bool isActive);
        Task<Dictionary<string, List<NotificationTemplate>>> GetAllTemplatesAsync(Guid tenantId);
        Task SeedDefaultTemplatesAsync(Guid tenantId);
    }
}
