using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareSphere.Data;
using CareSphere.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CareSphere.BackgroundServices
{
    public class LabNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LabNotificationBackgroundService> _logger;

        public LabNotificationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<LabNotificationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Lab Notification Background Service is starting.");

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(60));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation("Checking for completed lab reports with pending notifications...");

                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            var notificationService = scope.ServiceProvider.GetRequiredService<ILabNotificationService>();

                            // Query LabReports where NotificationSentAt is null and requisition status is Completed
                            var pendingReports = await context.LabReports
                                .IgnoreQueryFilters()
                                .Include(r => r.Requisition)
                                .Where(r => r.NotificationSentAt == null && r.Requisition.Status == "Completed")
                                .ToListAsync(stoppingToken);

                            foreach (var report in pendingReports)
                            {
                                try
                                {
                                    CareSphere.Infrastructure.TenantContext.BypassTenantId = report.TenantId;
                                    _logger.LogInformation($"Sending report notifications for Report ID: {report.Id}, Requisition: {report.Requisition.RequisitionNumber}");
                                    await notificationService.SendReportNotificationsAsync(report.TenantId, report.Id);
                                }
                                finally
                                {
                                    CareSphere.Infrastructure.TenantContext.BypassTenantId = null;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred during the pending lab notification check execution.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Background service is stopping
            }

            _logger.LogInformation("Lab Notification Background Service is stopping.");
        }
    }
}
