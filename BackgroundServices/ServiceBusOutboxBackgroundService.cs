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
    public class ServiceBusOutboxBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ServiceBusOutboxBackgroundService> _logger;

        public ServiceBusOutboxBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ServiceBusOutboxBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service Bus Outbox Background Service is starting.");

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogDebug("Processing pending Service Bus outbox messages...");

                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var serviceBusService = scope.ServiceProvider.GetRequiredService<IServiceBusService>();
                            await serviceBusService.ProcessOutboxAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred during Service Bus outbox processing execution.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Background service is stopping
            }

            _logger.LogInformation("Service Bus Outbox Background Service is stopping.");
        }
    }
}
