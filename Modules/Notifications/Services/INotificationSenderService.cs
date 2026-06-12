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
using System.Threading.Tasks;
using CareSphere.Models;
using CareSphere.Modules.Shared.Events;

namespace CareSphere.Modules.Notifications.Services
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
        Task SendLabReportReadyAsync(LabReportReady evt);
    }
}
