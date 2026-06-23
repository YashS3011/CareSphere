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

namespace CareSphere.Modules.Notifications.Services
{
    public class NotificationDashboardService : INotificationDashboardService
    {
        private readonly ApplicationDbContext _context;

        public NotificationDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationDashboardStats> GetDashboardStatsAsync(Guid tenantId)
        {
            var todayUtc = DateTime.UtcNow.Date;

            var smsSent = await _context.NotificationLogs
                .CountAsync(l => l.TenantId == tenantId && l.Channel == "SMS" && (l.Status == "Sent" || l.Status == "Delivered") && l.CreatedAt >= todayUtc);

            var whatsappSent = await _context.NotificationLogs
                .CountAsync(l => l.TenantId == tenantId && l.Channel == "WhatsApp" && (l.Status == "Sent" || l.Status == "Delivered") && l.CreatedAt >= todayUtc);

            var voiceSent = await _context.NotificationLogs
                .CountAsync(l => l.TenantId == tenantId && l.Channel == "Voice" && (l.Status == "Sent" || l.Status == "Delivered") && l.CreatedAt >= todayUtc);

            var inAppSent = await _context.NotificationLogs
                .CountAsync(l => l.TenantId == tenantId && l.Channel == "InApp" && (l.Status == "Sent" || l.Status == "Delivered") && l.CreatedAt >= todayUtc);

            var totalSent = smsSent + whatsappSent + voiceSent + inAppSent;

            var totalFailed = await _context.NotificationLogs
                .CountAsync(l => l.TenantId == tenantId && l.Status == "Failed" && l.CreatedAt >= todayUtc);

            var totalPending = await _context.AppointmentReminders
                .CountAsync(r => r.TenantId == tenantId && r.Status == "Scheduled");

            double deliveryRate = 100.0;
            if (totalSent + totalFailed > 0)
            {
                deliveryRate = (double)totalSent / (totalSent + totalFailed) * 100.0;
            }

            return new NotificationDashboardStats
            {
                TotalSentToday = totalSent,
                TotalFailedToday = totalFailed,
                TotalPendingReminders = totalPending,
                SmsSentToday = smsSent,
                WhatsAppSentToday = whatsappSent,
                VoiceSentToday = voiceSent,
                InAppSentToday = inAppSent,
                DeliveryRatePercent = Math.Round(deliveryRate, 2)
            };
        }

        public async Task<List<NotificationLog>> GetRecentLogsAsync(Guid tenantId)
        {
            return await _context.NotificationLogs.AsNoTracking()
                .Include(l => l.Patient)
                .Include(l => l.Doctor)
                .Where(l => l.TenantId == tenantId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task<List<NotificationLog>> GetFailedLogsAsync(Guid tenantId)
        {
            return await _context.NotificationLogs.AsNoTracking()
                .Include(l => l.Patient)
                .Include(l => l.Doctor)
                .Where(l => l.TenantId == tenantId && l.Status == "Failed" && l.RetryCount < l.MaxRetries)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}
