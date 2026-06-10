using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Services
{
    public interface INotificationDashboardService
    {
        Task<NotificationDashboardStats> GetDashboardStatsAsync(Guid tenantId);
        Task<List<NotificationLog>> GetRecentLogsAsync(Guid tenantId);
        Task<List<NotificationLog>> GetFailedLogsAsync(Guid tenantId);
    }
}
