using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Services
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
