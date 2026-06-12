using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CareSphere.Modules.Shared.Events;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Laboratory.Services;

namespace CareSphere.BackgroundServices
{
    public class ServiceBusConsumerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ServiceBusConsumerService> _logger;
        private ServiceBusClient? _client;
        private ServiceBusProcessor? _processor;

        public ServiceBusConsumerService(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<ServiceBusConsumerService> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionString = _configuration["AzureServiceBus:ConnectionString"];
            var queueName = _configuration["AzureServiceBus:QueueName"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // Service Bus consumer disabled falling back to background polling
                _logger.LogInformation("Service Bus consumer disabled falling back to background polling.");
                return;
            }

            try
            {
                _client = new ServiceBusClient(connectionString);
                _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

                _processor.ProcessMessageAsync += MessageHandler;
                _processor.ProcessErrorAsync += ErrorHandler;

                await _processor.StartProcessingAsync(stoppingToken);

                // Wait until cancellation is requested
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }

                await _processor.StopProcessingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Azure Service Bus processor execution.");
            }
            finally
            {
                if (_processor != null)
                {
                    await _processor.DisposeAsync();
                }
                if (_client != null)
                {
                    await _client.DisposeAsync();
                }
            }
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation($"Received Service Bus message: {body}");

            try
            {
                var envelope = JsonSerializer.Deserialize<ServiceBusEnvelope>(body);
                if (envelope == null)
                {
                    _logger.LogWarning("Received empty or malformed message envelope.");
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                if (envelope.MessageType.Equals("AppointmentReminder", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = JsonSerializer.Deserialize<AppointmentReminderPayload>(envelope.Payload);
                    if (payload != null)
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IAppointmentReminderService>();
                        await service.ProcessAppointmentReminderAsync(payload.ReminderId);
                    }
                }
                else if (envelope.MessageType.Equals("LabReportReady", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = JsonSerializer.Deserialize<LabReportReady>(envelope.Payload);
                    if (payload != null)
                    {
                        var service = scope.ServiceProvider.GetRequiredService<ILabNotificationService>();
                        await service.SendReportNotificationsAsync(payload.TenantId, payload.LabReportId);

                        var senderService = scope.ServiceProvider.GetRequiredService<INotificationSenderService>();
                        await senderService.SendLabReportReadyAsync(payload);
                    }
                }
                else if (envelope.MessageType.Equals("DischargeNotification", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = JsonSerializer.Deserialize<DischargeNotificationPayload>(envelope.Payload);
                    if (payload != null)
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IDischargeNotificationService>();
                        await service.SendDischargeNotificationAsync(payload.TenantId, payload.PatientId, payload.AllotmentId, payload.DischargedAt, payload.Language);
                    }
                }
                else if (envelope.MessageType.Equals("EncounterCompleted", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = JsonSerializer.Deserialize<EncounterCompleted>(envelope.Payload);
                    if (payload != null)
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                        await service.CreateDraftFromEncounterAsync(payload);
                    }
                }
                else if (envelope.MessageType.Equals("DispenseCompleted", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = JsonSerializer.Deserialize<DispenseCompleted>(envelope.Payload);
                    if (payload != null)
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                        await service.AddDispenseLineItemAsync(payload);
                    }
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Service Bus message. Abandoning message for retry.");
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, $"Service Bus processor encountered an error. Source: {args.ErrorSource}, Queue: {args.EntityPath}");
            return Task.CompletedTask;
        }
    }

    // Helper classes for serialization
    public class ServiceBusEnvelope
    {
        public string MessageType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }

    public class AppointmentReminderPayload
    {
        public Guid ReminderId { get; set; }
    }

    public class LabReportReadyPayload
    {
        public Guid TenantId { get; set; }
        public Guid LabReportId { get; set; }
    }

    public class DischargeNotificationPayload
    {
        public Guid TenantId { get; set; }
        public Guid PatientId { get; set; }
        public Guid? AllotmentId { get; set; }
        public DateTime DischargedAt { get; set; }
        public string Language { get; set; } = "en";
    }
}
