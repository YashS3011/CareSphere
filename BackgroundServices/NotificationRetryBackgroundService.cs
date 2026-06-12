using System;
using System.Threading;
using System.Threading.Tasks;
using CareSphere.Modules.Clinical.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Patients.Services;
using CareSphere.Modules.Ward.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CareSphere.BackgroundServices
{
    public class NotificationRetryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationRetryBackgroundService> _logger;

        public NotificationRetryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationRetryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Retry Background Service is starting.");

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation("Retrying failed notifications...");

                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var senderService = scope.ServiceProvider.GetRequiredService<INotificationSenderService>();
                            await senderService.RetryFailedNotificationsAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred during failed notification retries execution.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Background service is stopping
            }

            _logger.LogInformation("Notification Retry Background Service is stopping.");
        }
    }
}
