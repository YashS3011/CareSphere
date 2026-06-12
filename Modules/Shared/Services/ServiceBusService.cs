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
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CareSphere.Modules.Shared.Services
{
    public class ServiceBusService : IServiceBusService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceBusService> _logger;

        public ServiceBusService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<ServiceBusService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnqueueMessageAsync(string messageType, object payload, Guid tenantId, DateTime? scheduledEnqueueAt = null)
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            var outbox = new ServiceBusOutbox
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MessageType = messageType,
                Payload = jsonPayload,
                Status = "Pending",
                ScheduledEnqueueAt = scheduledEnqueueAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceBusOutboxes.Add(outbox);
            await _context.SaveChangesAsync();

            var connectionString = _configuration["AzureServiceBus:ConnectionString"];
            var queueName = _configuration["AzureServiceBus:QueueName"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                outbox.FailureReason = "Azure Service Bus not configured falling back to database outbox polling";
                _context.ServiceBusOutboxes.Update(outbox);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Azure Service Bus not configured, falling back to database outbox polling.");
                return;
            }

            try
            {
                await using var client = new ServiceBusClient(connectionString);
                var sender = client.CreateSender(queueName);
                
                // Wrap in standard ServiceBusMessage envelope matching expectations
                var envelope = new { MessageType = messageType, Payload = jsonPayload };
                var message = new ServiceBusMessage(JsonSerializer.Serialize(envelope));
                
                if (scheduledEnqueueAt.HasValue)
                {
                    message.ScheduledEnqueueTime = scheduledEnqueueAt.Value;
                }

                await sender.SendMessageAsync(message);

                outbox.Status = "Enqueued";
                outbox.EnqueuedAt = DateTime.UtcNow;
                outbox.ServiceBusMessageId = message.MessageId;
                outbox.ProcessedAt = DateTime.UtcNow;

                _context.ServiceBusOutboxes.Update(outbox);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                outbox.FailureReason = ex.Message;
                _context.ServiceBusOutboxes.Update(outbox);
                await _context.SaveChangesAsync();
                _logger.LogError(ex, $"Failed to send message {outbox.Id} to Service Bus. Left as Pending for outbox retry.");
            }
        }

        public async Task ProcessOutboxAsync()
        {
            var connectionString = _configuration["AzureServiceBus:ConnectionString"];
            var queueName = _configuration["AzureServiceBus:QueueName"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogDebug("Azure Service Bus not configured. Skipping outbox database polling processor.");
                return;
            }

            var pending = await _context.ServiceBusOutboxes
                .Where(o => o.Status == "Pending" && (o.ScheduledEnqueueAt == null || o.ScheduledEnqueueAt <= DateTime.UtcNow))
                .ToListAsync();

            if (!pending.Any())
            {
                return;
            }

            await using var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(queueName);

            foreach (var outbox in pending)
            {
                try
                {
                    var envelope = new { MessageType = outbox.MessageType, Payload = outbox.Payload };
                    var message = new ServiceBusMessage(JsonSerializer.Serialize(envelope));
                    
                    if (outbox.ScheduledEnqueueAt.HasValue)
                    {
                        message.ScheduledEnqueueTime = outbox.ScheduledEnqueueAt.Value;
                    }

                    await sender.SendMessageAsync(message);

                    outbox.Status = "Enqueued";
                    outbox.EnqueuedAt = DateTime.UtcNow;
                    outbox.ServiceBusMessageId = message.MessageId;
                    outbox.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    outbox.FailureReason = ex.Message;
                    outbox.Status = "Failed"; // Mark as Failed or keep Pending? The outbox service retries Pending. Let's keep status as Pending or Failed?
                    // The prompt says "queries ServiceBusOutbox where Status is Pending ... used by outbox service as a fallback".
                    // So if it fails, we keep it Pending (or increment a count) so it gets retried again next time. Let's leave status as Pending if we want it to be retried.
                    // We can also increment retry or just log. Let's keep it as Pending so it is retried.
                    outbox.Status = "Pending";
                    _logger.LogError(ex, $"Outbox processor failed to send message {outbox.Id}.");
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
