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
using CareSphere.Data;
using CareSphere.Models;
using CareSphere.Modules.Shared.Events;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
                            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                            var connectionString = configuration["AzureServiceBus:ConnectionString"];

                            if (string.IsNullOrWhiteSpace(connectionString))
                            {
                                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                await ProcessOutboxLocallyAsync(dbContext, scope.ServiceProvider);
                            }
                            else
                            {
                                var serviceBusService = scope.ServiceProvider.GetRequiredService<IServiceBusService>();
                                await serviceBusService.ProcessOutboxAsync();
                            }
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

        private async Task ProcessOutboxLocallyAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var pending = await dbContext.ServiceBusOutboxes
                .Where(o => o.Status == "Pending" && (o.ScheduledEnqueueAt == null || o.ScheduledEnqueueAt <= DateTime.UtcNow))
                .ToListAsync();

            if (!pending.Any())
            {
                return;
            }

            foreach (var outbox in pending)
            {
                try
                {
                    switch (outbox.MessageType)
                    {
                        case "EncounterCompleted":
                            var encounterEvt = JsonSerializer.Deserialize<EncounterCompleted>(outbox.Payload);
                            if (encounterEvt != null)
                            {
                                var service = serviceProvider.GetRequiredService<IInvoiceService>();
                                await service.CreateDraftFromEncounterAsync(encounterEvt);
                            }
                            break;

                        case "DispenseCompleted":
                            var dispenseEvt = JsonSerializer.Deserialize<DispenseCompleted>(outbox.Payload);
                            if (dispenseEvt != null)
                            {
                                var service = serviceProvider.GetRequiredService<IInvoiceService>();
                                await service.AddDispenseLineItemAsync(dispenseEvt);
                            }
                            break;

                        case "LabReportReady":
                            var labEvt = JsonSerializer.Deserialize<LabReportReady>(outbox.Payload);
                            if (labEvt != null)
                            {
                                var service = serviceProvider.GetRequiredService<INotificationSenderService>();
                                await service.SendLabReportReadyAsync(labEvt);
                            }
                            break;

                        case "AppointmentBooked":
                            var apptEvt = JsonSerializer.Deserialize<AppointmentBookedEvent>(outbox.Payload);
                            if (apptEvt != null)
                            {
                                var service = serviceProvider.GetRequiredService<INotificationSenderService>();
                                await service.SendAppointmentConfirmationAsync(apptEvt);
                            }
                            break;

                        case "PatientAdmitted":
                            // G11.1: Patient admitted — log handled; future: start bed-day billing timer
                            _logger.LogInformation($"Patient admitted event processed for allotment {outbox.Payload}.");
                            break;

                        case "PatientDischarged":
                            // G11.2: Patient discharged — route to discharge notification service
                            var dischargedEvt2 = JsonSerializer.Deserialize<PatientDischarged>(outbox.Payload);
                            if (dischargedEvt2 != null)
                            {
                                var notifService = serviceProvider.GetRequiredService<INotificationSenderService>();
                                await notifService.SendPatientDischargedAsync(dischargedEvt2);
                            }
                            break;

                        case "CriticalResultFlagged":
                            // G11.3: Critical lab result — alert ordering doctor immediately
                            var critEvt = JsonSerializer.Deserialize<CriticalResultFlagged>(outbox.Payload);
                            if (critEvt != null)
                            {
                                var notifService = serviceProvider.GetRequiredService<INotificationSenderService>();
                                await notifService.SendCriticalResultAlertAsync(critEvt);
                            }
                            break;

                        case "LabRequisitionCreated":
                            // G11.4: Lab requisition created — auto-add test fees to invoice
                            var labReqEvt = JsonSerializer.Deserialize<LabRequisitionCreated>(outbox.Payload);
                            if (labReqEvt != null)
                            {
                                var invoiceService = serviceProvider.GetRequiredService<IInvoiceService>();
                                await invoiceService.AddLabRequisitionLineItemsAsync(labReqEvt);
                            }
                            break;

                        case "PatientArrived":
                            var arrivedEvt = JsonSerializer.Deserialize<PatientArrived>(outbox.Payload);
                            if (arrivedEvt != null)
                            {
                                var queueService = serviceProvider.GetRequiredService<IQueueService>();
                                await queueService.AddToQueueAsync(new DoctorQueueEntry
                                {
                                    DoctorId = arrivedEvt.DoctorId,
                                    PatientId = arrivedEvt.PatientId,
                                    TenantId = arrivedEvt.TenantId,
                                    TriagePriority = "Routine",
                                    CheckedInAt = DateTime.UtcNow,
                                    Status = "Waiting"
                                });
                            }
                            break;

                        default:
                            _logger.LogWarning($"Unknown MessageType '{outbox.MessageType}' for outbox message {outbox.Id}.");
                            break;
                    }

                    outbox.Status = "Enqueued";
                    outbox.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    outbox.FailureReason = ex.Message;
                    _logger.LogError(ex, $"Failed to process outbox message locally. Message ID: {outbox.Id}, Type: {outbox.MessageType}");
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
