using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareSphere.Data;
using CareSphere.Modules.Billing.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CareSphere.BackgroundServices
{
    public class BedChargeBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BedChargeBackgroundService> _logger;

        public BedChargeBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BedChargeBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bed Charge Auto-Billing Background Service is starting.");

            // Small initial delay on startup
            await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Processing daily bed charges for active IPD allotments...");

                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();

                        // Fetch all active IPD bed allotments across all tenants
                        var activeAllotments = await context.BedAllotments
                            .IgnoreQueryFilters()
                            .Include(a => a.Bed)
                            .Where(a => a.Status == "Active" && a.AdmissionType == "IPD")
                            .ToListAsync(stoppingToken);

                        foreach (var allotment in activeAllotments)
                        {
                            try
                            {
                                if (allotment.Bed != null && allotment.Bed.DailyChargeAmount > 0)
                                {
                                    CareSphere.Infrastructure.TenantContext.BypassTenantId = allotment.TenantId;
                                    _logger.LogInformation($"Applying daily bed charge: {allotment.Bed.DailyChargeAmount} for patient {allotment.PatientId} (Tenant: {allotment.TenantId})");
                                    await invoiceService.AddDailyBedChargeAsync(
                                        allotment.TenantId,
                                        allotment.PatientId,
                                        allotment.EncounterId,
                                        allotment.Bed.DailyChargeAmount,
                                        allotment.Bed.BedNumber
                                    );
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error applying bed charge for allotment {allotment.Id}");
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
                    _logger.LogError(ex, "An error occurred during bed charge background service run.");
                }

                // Wait 24 hours
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Shutdown requested
                }
            }

            _logger.LogInformation("Bed Charge Auto-Billing Background Service is stopping.");
        }
    }
}
