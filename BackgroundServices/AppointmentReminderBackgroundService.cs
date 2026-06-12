using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareSphere.Data;
using CareSphere.Modules.Clinical.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Patients.Services;
using CareSphere.Modules.Ward.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CareSphere.BackgroundServices
{
    public class AppointmentReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AppointmentReminderBackgroundService> _logger;

        public AppointmentReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<AppointmentReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Appointment Reminder Background Service is starting.");

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation("Checking for scheduled appointment reminders to send...");

                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            var reminderService = scope.ServiceProvider.GetRequiredService<IAppointmentReminderService>();

                            var now = DateTime.UtcNow;
                            var pendingReminders = await context.AppointmentReminders
                                .IgnoreQueryFilters()
                                .Where(r => r.Status == "Scheduled" && r.ScheduledAt <= now)
                                .ToListAsync(stoppingToken);

                            foreach (var reminder in pendingReminders)
                            {
                                try
                                {
                                    CareSphere.Infrastructure.TenantContext.BypassTenantId = reminder.TenantId;
                                    _logger.LogInformation($"Sending scheduled reminder for Reminder ID: {reminder.Id}, Patient ID: {reminder.PatientId}");
                                    await reminderService.ProcessAppointmentReminderAsync(reminder.Id);
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
                        _logger.LogError(ex, "An error occurred during scheduled appointment reminders execution.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Background service is stopping
            }

            _logger.LogInformation("Appointment Reminder Background Service is stopping.");
        }
    }
}
