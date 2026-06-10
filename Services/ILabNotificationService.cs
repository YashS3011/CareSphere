using CareSphere.Models;

namespace CareSphere.Services
{
    public interface ILabNotificationService
    {
        Task SendReportNotificationsAsync(Guid tenantId, Guid labReportId);
        Task<List<InAppNotification>> GetUnreadNotificationsAsync(string recipientType, string recipientId);
        Task MarkNotificationReadAsync(Guid notificationId);
        Task<int> GetNotificationCountAsync(string recipientType, string recipientId);
    }
}
