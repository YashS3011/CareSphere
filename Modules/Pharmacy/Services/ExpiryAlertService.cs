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
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace CareSphere.Modules.Pharmacy.Services
{
    public class ExpiryAlertService : IExpiryAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;

        public ExpiryAlertService(ApplicationDbContext context, IConfiguration configuration, IAuditService auditService)
        {
            _context = context;
            _configuration = configuration;
            _auditService = auditService;
        }

        public async Task CheckAndGenerateExpiryAlertsAsync(Guid tenantId)
        {
            // Find all active batches for the tenant expiring in the next 90 days with current stock > 0
            var expirationThreshold = DateTime.UtcNow.AddDays(90);
            var expiringBatches = await _context.PharmacyBatches
                .Include(b => b.Item)
                .Where(b => b.TenantId == tenantId && 
                            b.IsActive && 
                            b.CurrentStock > 0 && 
                            b.ExpiryDate <= expirationThreshold)
                .ToListAsync();

            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromPhone = _configuration["Twilio:FromPhone"];
            var alertPhoneNumber = _configuration["Pharmacy:ExpiryAlertPhoneNumber"];

            var isTwilioConfigured = !string.IsNullOrWhiteSpace(accountSid) && 
                                     !string.IsNullOrWhiteSpace(authToken) && 
                                     !string.IsNullOrWhiteSpace(fromPhone) && 
                                     !string.IsNullOrWhiteSpace(alertPhoneNumber);

            if (isTwilioConfigured)
            {
                try
                {
                    TwilioClient.Init(accountSid, authToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ExpiryAlertService] Twilio initialization failed: {ex.Message}");
                    isTwilioConfigured = false;
                }
            }

            foreach (var batch in expiringBatches)
            {
                // Cooldown: 7 days between alerts for the same batch
                if (batch.ExpiryAlertSentAt.HasValue && 
                    (DateTime.UtcNow - batch.ExpiryAlertSentAt.Value).TotalDays < 7)
                {
                    continue;
                }

                var daysUntilExpiry = (int)(batch.ExpiryDate.Date - DateTime.UtcNow.Date).TotalDays;
                if (daysUntilExpiry < 0) daysUntilExpiry = 0;

                // Create In-App Alert
                var inAppAlert = new ExpiryAlert
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    BatchId = batch.Id,
                    ItemId = batch.ItemId,
                    ExpiryDate = batch.ExpiryDate,
                    DaysUntilExpiry = daysUntilExpiry,
                    AlertType = "InApp",
                    SentAt = DateTime.UtcNow,
                    IsAcknowledged = false
                };

                _context.ExpiryAlerts.Add(inAppAlert);

                // Create SMS Alert if Twilio is configured
                if (isTwilioConfigured)
                {
                    try
                    {
                        var smsBody = $"CareSphere Alert: Pharmacy batch '{batch.BatchNumber}' for item '{batch.Item.ItemName}' is expiring in {daysUntilExpiry} days on {batch.ExpiryDate:yyyy-MM-dd}. Stock: {batch.CurrentStock} {batch.Item.Unit}.";
                        
                        var message = await MessageResource.CreateAsync(
                            body: smsBody,
                            from: new PhoneNumber(fromPhone),
                            to: new PhoneNumber(alertPhoneNumber)
                        );

                        if (message != null && !string.IsNullOrEmpty(message.Sid))
                        {
                            var smsAlert = new ExpiryAlert
                            {
                                Id = Guid.NewGuid(),
                                TenantId = tenantId,
                                BatchId = batch.Id,
                                ItemId = batch.ItemId,
                                ExpiryDate = batch.ExpiryDate,
                                DaysUntilExpiry = daysUntilExpiry,
                                AlertType = "SMS",
                                SentAt = DateTime.UtcNow,
                                IsAcknowledged = true // SMS is considered delivered directly
                            };
                            _context.ExpiryAlerts.Add(smsAlert);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ExpiryAlertService] Failed to send SMS for batch {batch.BatchNumber}: {ex.Message}");
                    }
                }

                batch.ExpiryAlertSentAt = DateTime.UtcNow;
                _context.PharmacyBatches.Update(batch);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<ExpiryAlert>> GetUnacknowledgedAlertsAsync(Guid tenantId)
        {
            return await _context.ExpiryAlerts
                .Include(a => a.Batch)
                .Include(a => a.Item)
                .Where(a => a.TenantId == tenantId && !a.IsAcknowledged && a.AlertType == "InApp")
                .OrderBy(a => a.DaysUntilExpiry)
                .ToListAsync();
        }

        public async Task<List<ExpiryAlert>> GetAllAlertsAsync(Guid tenantId)
        {
            return await _context.ExpiryAlerts
                .Include(a => a.Batch)
                .Include(a => a.Item)
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.SentAt)
                .ToListAsync();
        }

        public async Task AcknowledgeAlertAsync(Guid alertId, string acknowledgedByUserId)
        {
            var alert = await _context.ExpiryAlerts.FindAsync(alertId);
            if (alert == null)
                throw new InvalidOperationException("Expiry alert not found.");

            alert.IsAcknowledged = true;
            alert.AcknowledgedAt = DateTime.UtcNow;
            alert.AcknowledgedByUserId = acknowledgedByUserId;

            _context.ExpiryAlerts.Update(alert);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = acknowledgedByUserId,
                Action = "EXPIRY_ALERT_ACKNOWLEDGED",
                ResourceType = "ExpiryAlert",
                ResourceId = alert.Id.ToString(),
                TenantId = alert.TenantId
            });
        }
    }
}
