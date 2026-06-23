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
    public class ExpiryAlertBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiryAlertBackgroundService> _logger;

        public ExpiryAlertBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ExpiryAlertBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expiry Alert Background Service is starting.");

            // Small initial delay on startup
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking batch expiries for all active tenants...");

                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var expiryService = scope.ServiceProvider.GetRequiredService<IExpiryAlertService>();

                        // Discover all active tenants that have pharmacy items
                        var tenantIds = await context.PharmacyItems
                            .IgnoreQueryFilters()
                            .Select(pi => pi.TenantId)
                            .Distinct()
                            .ToListAsync(stoppingToken);

                        foreach (var tenantId in tenantIds)
                        {
                            try
                            {
                                CareSphere.Infrastructure.TenantContext.BypassTenantId = tenantId;
                                _logger.LogInformation($"Running expiry check for Tenant: {tenantId}");
                                await expiryService.CheckAndGenerateExpiryAlertsAsync(tenantId);
                                
                                _logger.LogInformation($"Running stock reorder level check for Tenant: {tenantId}");
                                await expiryService.CheckAndGenerateReorderAlertsAsync(tenantId);
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
                    _logger.LogError(ex, "An error occurred during the expiry check execution.");
                }

                // Wait 24 hours before next execution
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Shutdown requested
                }
            }

            _logger.LogInformation("Expiry Alert Background Service is stopping.");
        }
    }
}
