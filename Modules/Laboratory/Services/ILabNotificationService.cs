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
using CareSphere.Models;

namespace CareSphere.Modules.Laboratory.Services
{
    public interface ILabNotificationService
    {
        Task SendReportNotificationsAsync(Guid tenantId, Guid labReportId);
        Task<List<InAppNotification>> GetUnreadNotificationsAsync(string recipientType, string recipientId);
        Task MarkNotificationReadAsync(Guid notificationId);
        Task<int> GetNotificationCountAsync(string recipientType, string recipientId);
    }
}
