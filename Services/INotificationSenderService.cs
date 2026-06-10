using System;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Services
{
    public interface INotificationSenderService
    {
        Task SendSmsAsync(
            Guid tenantId,
            string recipientPhone,
            string messageBody,
            string notificationType,
            Guid? patientId = null,
            Guid? doctorId = null,
            string language = "en",
            DateTime? scheduledAt = null);

        Task SendWhatsAppAsync(
            Guid tenantId,
            string recipientPhone,
            string messageBody,
            string notificationType,
            Guid? patientId = null,
            Guid? doctorId = null,
            string language = "en",
            DateTime? scheduledAt = null);

        Task SendVoiceReminderAsync(
            Guid tenantId,
            string recipientPhone,
            string messageBody,
            string language,
            Guid? patientId = null,
            string? notificationType = null);

        Task SendInAppAsync(
            Guid tenantId,
            string recipientType,
            string recipientId,
            string title,
            string messageBody,
            string resourceType,
            string resourceId,
            Guid? patientId = null,
            string? notificationType = null);

        Task RetryFailedNotificationsAsync();
        Task<bool> RetryLogAsync(Guid logId);
    }
}
