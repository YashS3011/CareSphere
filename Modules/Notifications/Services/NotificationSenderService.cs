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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using CareSphere.Modules.Shared.Events;

namespace CareSphere.Modules.Notifications.Services
{
    public class NotificationSenderService : INotificationSenderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;
        private readonly ILogger<NotificationSenderService> _logger;

        public NotificationSenderService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IAuditService auditService,
            ILogger<NotificationSenderService> logger)
        {
            _context = context;
            _configuration = configuration;
            _auditService = auditService;
            _logger = logger;
        }

        private bool InitTwilio()
        {
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken))
            {
                return false;
            }
            try
            {
                TwilioClient.Init(accountSid, authToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Twilio client.");
                return false;
            }
        }

        private async Task<string?> GetPatientNameAsync(Guid? patientId)
        {
            if (!patientId.HasValue) return null;
            var patient = await _context.Patients.FindAsync(patientId.Value);
            return patient != null ? $"{patient.FirstName} {patient.LastName}" : null;
        }

        public async Task SendSmsAsync(
            Guid tenantId,
            string recipientPhone,
            string messageBody,
            string notificationType,
            Guid? patientId = null,
            Guid? doctorId = null,
            string language = "en",
            DateTime? scheduledAt = null)
        {
            // Check PatientPreferences for OptOutSms
            if (patientId.HasValue)
            {
                var pref = await _context.PatientPreferences.FirstOrDefaultAsync(p => p.PatientId == patientId.Value);
                if (pref != null && pref.OptOutSms)
                {
                    var skippedLog = new NotificationLog
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        PatientId = patientId,
                        DoctorId = doctorId,
                        RecipientPhone = recipientPhone,
                        RecipientName = await GetPatientNameAsync(patientId),
                        Channel = "SMS",
                        NotificationType = notificationType,
                        Language = language,
                        MessageBody = messageBody,
                        Status = "Skipped",
                        ScheduledAt = scheduledAt,
                        SentAt = DateTime.UtcNow,
                        FailureReason = "Patient opted out of SMS notifications",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.NotificationLogs.Add(skippedLog);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"SMS skipped for Patient {patientId} because they opted out.");
                    return;
                }
            }

            var log = new NotificationLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PatientId = patientId,
                DoctorId = doctorId,
                RecipientPhone = recipientPhone,
                RecipientName = await GetPatientNameAsync(patientId),
                Channel = "SMS",
                NotificationType = notificationType,
                Language = language,
                MessageBody = messageBody,
                Status = "Pending",
                ScheduledAt = scheduledAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();

            var fromPhone = _configuration["Twilio:FromPhone"];
            if (!InitTwilio() || string.IsNullOrWhiteSpace(fromPhone))
            {
                log.Status = "Failed";
                log.FailureReason = "Twilio credentials or FromPhone not configured";
                _context.NotificationLogs.Update(log);
                await _context.SaveChangesAsync();
                _logger.LogWarning("SMS sending failed: Twilio not configured.");
                return;
            }

            try
            {
                var response = await MessageResource.CreateAsync(
                    body: messageBody,
                    from: new PhoneNumber(fromPhone),
                    to: new PhoneNumber(recipientPhone)
                );

                if (response != null && !string.IsNullOrEmpty(response.Sid))
                {
                    log.Status = "Sent";
                    log.ProviderMessageId = response.Sid;
                    log.ProviderResponse = response.Status?.ToString();
                    log.SentAt = DateTime.UtcNow;
                }
                else
                {
                    log.Status = "Failed";
                    log.FailureReason = "Twilio returned empty SID";
                }
            }
            catch (Exception ex)
            {
                log.Status = "Failed";
                log.FailureReason = ex.Message;
                _logger.LogError(ex, $"Failed to send SMS to {recipientPhone}");
            }

            _context.NotificationLogs.Update(log);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "SMS_NOTIFICATION_SENT",
                ResourceType = "NotificationLog",
                ResourceId = log.Id.ToString(),
                TenantId = tenantId
            });
        }

        public async Task SendWhatsAppAsync(
            Guid tenantId,
            string recipientPhone,
            string messageBody,
            string notificationType,
            Guid? patientId = null,
            Guid? doctorId = null,
            string language = "en",
            DateTime? scheduledAt = null)
        {
            // Check PatientPreferences for OptOutWhatsApp
            if (patientId.HasValue)
            {
                var pref = await _context.PatientPreferences.FirstOrDefaultAsync(p => p.PatientId == patientId.Value);
                if (pref != null && pref.OptOutWhatsApp)
                {
                    var skippedLog = new NotificationLog
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        PatientId = patientId,
                        DoctorId = doctorId,
                        RecipientPhone = recipientPhone,
                        RecipientName = await GetPatientNameAsync(patientId),
                        Channel = "WhatsApp",
                        NotificationType = notificationType,
                        Language = language,
                        MessageBody = messageBody,
                        Status = "Skipped",
                        ScheduledAt = scheduledAt,
                        SentAt = DateTime.UtcNow,
                        FailureReason = "Patient opted out of WhatsApp notifications",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.NotificationLogs.Add(skippedLog);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"WhatsApp skipped for Patient {patientId} because they opted out.");
                    return;
                }
            }

            var log = new NotificationLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PatientId = patientId,
                DoctorId = doctorId,
                RecipientPhone = recipientPhone,
                RecipientName = await GetPatientNameAsync(patientId),
                Channel = "WhatsApp",
                NotificationType = notificationType,
                Language = language,
                MessageBody = messageBody,
                Status = "Pending",
                ScheduledAt = scheduledAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();

            var fromPhone = _configuration["Twilio:WhatsAppFromNumber"];
            if (!InitTwilio() || string.IsNullOrWhiteSpace(fromPhone))
            {
                log.Status = "Failed";
                log.FailureReason = "Twilio credentials or WhatsAppFromNumber not configured";
                _context.NotificationLogs.Update(log);
                await _context.SaveChangesAsync();
                _logger.LogWarning("WhatsApp sending failed: Twilio or WhatsAppFromNumber not configured.");
                return;
            }

            try
            {
                var fromStr = fromPhone.StartsWith("whatsapp:") ? fromPhone : $"whatsapp:{fromPhone}";
                var toStr = recipientPhone.StartsWith("whatsapp:") ? recipientPhone : $"whatsapp:{recipientPhone}";

                var response = await MessageResource.CreateAsync(
                    body: messageBody,
                    from: new PhoneNumber(fromStr),
                    to: new PhoneNumber(toStr)
                );

                if (response != null && !string.IsNullOrEmpty(response.Sid))
                {
                    log.Status = "Sent";
                    log.ProviderMessageId = response.Sid;
                    log.ProviderResponse = response.Status?.ToString();
                    log.SentAt = DateTime.UtcNow;
                }
                else
                {
                    log.Status = "Failed";
                    log.FailureReason = "Twilio WhatsApp returned empty SID";
                }
            }
            catch (Exception ex)
            {
                log.Status = "Failed";
                log.FailureReason = ex.Message;
                _logger.LogError(ex, $"Failed to send WhatsApp to {recipientPhone}");
            }

            _context.NotificationLogs.Update(log);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "WHATSAPP_NOTIFICATION_SENT",
                ResourceType = "NotificationLog",
                ResourceId = log.Id.ToString(),
                TenantId = tenantId
            });
        }

        #region FR-038 RECOMMENDED FEATURE BEGIN
        // ensure Twilio account has voice capability enabled and sufficient balance
        public async Task SendVoiceReminderAsync(
            Guid tenantId,
            string recipientPhone,
            string messageBody,
            string language,
            Guid? patientId = null,
            string? notificationType = null)
        {
            // Check PatientPreferences for OptOutVoice
            if (patientId.HasValue)
            {
                var pref = await _context.PatientPreferences.FirstOrDefaultAsync(p => p.PatientId == patientId.Value);
                if (pref != null && pref.OptOutVoice)
                {
                    var skippedLog = new NotificationLog
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        PatientId = patientId,
                        RecipientPhone = recipientPhone,
                        RecipientName = await GetPatientNameAsync(patientId),
                        Channel = "Voice",
                        NotificationType = notificationType ?? "AppointmentReminder",
                        Language = language,
                        MessageBody = messageBody,
                        Status = "Skipped",
                        FailureReason = "Patient opted out of Voice notifications",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.NotificationLogs.Add(skippedLog);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Voice reminder skipped for Patient {patientId} because they opted out.");
                    return;
                }
            }

            var log = new NotificationLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PatientId = patientId,
                RecipientPhone = recipientPhone,
                RecipientName = await GetPatientNameAsync(patientId),
                Channel = "Voice",
                NotificationType = notificationType ?? "AppointmentReminder",
                Language = language,
                MessageBody = messageBody,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();

            var fromPhone = _configuration["Twilio:FromPhone"];
            var baseUrl = _configuration["Twilio:BaseUrl"] ?? "http://localhost:5000";

            if (!InitTwilio() || string.IsNullOrWhiteSpace(fromPhone))
            {
                log.Status = "Failed";
                log.FailureReason = "Twilio credentials or FromPhone not configured";
                _context.NotificationLogs.Update(log);
                await _context.SaveChangesAsync();
                _logger.LogWarning("Voice call failed: Twilio not configured.");
                return;
            }

            try
            {
                var callbackUrl = $"{baseUrl.TrimEnd('/')}/api/twilio/twiml/{log.Id}";
                var call = await CallResource.CreateAsync(
                    to: new PhoneNumber(recipientPhone),
                    from: new PhoneNumber(fromPhone),
                    url: new Uri(callbackUrl)
                );

                if (call != null && !string.IsNullOrEmpty(call.Sid))
                {
                    log.Status = "Sent";
                    log.ProviderMessageId = call.Sid;
                    log.ProviderResponse = call.Status?.ToString();
                    log.SentAt = DateTime.UtcNow;
                }
                else
                {
                    log.Status = "Failed";
                    log.FailureReason = "Twilio Voice call returned empty SID";
                }
            }
            catch (Exception ex)
            {
                log.Status = "Failed";
                log.FailureReason = ex.Message;
                _logger.LogError(ex, $"Failed to make Twilio voice call to {recipientPhone}");
            }

            _context.NotificationLogs.Update(log);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "VOICE_NOTIFICATION_SENT",
                ResourceType = "NotificationLog",
                ResourceId = log.Id.ToString(),
                TenantId = tenantId
            });
        }
        #endregion FR-038 RECOMMENDED FEATURE END

        public async Task SendInAppAsync(
            Guid tenantId,
            string recipientType,
            string recipientId,
            string title,
            string messageBody,
            string resourceType,
            string resourceId,
            Guid? patientId = null,
            string? notificationType = null)
        {
            // Insert into the existing InAppNotifications table
            var inApp = new InAppNotification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RecipientType = recipientType,
                RecipientId = recipientId,
                Title = title,
                Message = messageBody,
                ResourceType = resourceType,
                ResourceId = resourceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.InAppNotifications.Add(inApp);

            // Log details
            Guid? patientGuid = patientId;
            Guid? doctorGuid = null;

            if (!patientGuid.HasValue && recipientType.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                if (Guid.TryParse(recipientId, out var parsedPatientId))
                {
                    patientGuid = parsedPatientId;
                }
            }
            if (recipientType.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                if (Guid.TryParse(recipientId, out var parsedDoctorId))
                {
                    doctorGuid = parsedDoctorId;
                }
            }

            var log = new NotificationLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PatientId = patientGuid,
                DoctorId = doctorGuid,
                RecipientPhone = null,
                RecipientName = recipientType.Equals("Patient", StringComparison.OrdinalIgnoreCase) ? await GetPatientNameAsync(patientGuid) : $"Doctor ({recipientId})",
                Channel = "InApp",
                NotificationType = notificationType ?? "GeneralAlert",
                Language = "en",
                MessageBody = messageBody,
                Status = "Sent",
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "IN_APP_NOTIFICATION_SENT",
                ResourceType = "NotificationLog",
                ResourceId = log.Id.ToString(),
                TenantId = tenantId
            });
        }

        public async Task RetryFailedNotificationsAsync()
        {
            var yesterday = DateTime.UtcNow.AddHours(-24);
            var failedLogs = await _context.NotificationLogs
                .Where(l => l.Status == "Failed" && l.RetryCount < l.MaxRetries && l.CreatedAt >= yesterday)
                .ToListAsync();

            if (!failedLogs.Any()) return;

            foreach (var log in failedLogs)
            {
                log.RetryCount++;
                try
                {
                    if (log.Channel.Equals("SMS", StringComparison.OrdinalIgnoreCase))
                    {
                        var fromPhone = _configuration["Twilio:FromPhone"];
                        if (InitTwilio() && !string.IsNullOrWhiteSpace(fromPhone) && !string.IsNullOrWhiteSpace(log.RecipientPhone))
                        {
                            var response = await MessageResource.CreateAsync(
                                body: log.MessageBody,
                                from: new PhoneNumber(fromPhone),
                                to: new PhoneNumber(log.RecipientPhone)
                            );
                            if (response != null && !string.IsNullOrEmpty(response.Sid))
                            {
                                log.Status = "Sent";
                                log.ProviderMessageId = response.Sid;
                                log.SentAt = DateTime.UtcNow;
                                log.FailureReason = null;
                            }
                        }
                    }
                    else if (log.Channel.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
                    {
                        var fromPhone = _configuration["Twilio:WhatsAppFromNumber"];
                        if (InitTwilio() && !string.IsNullOrWhiteSpace(fromPhone) && !string.IsNullOrWhiteSpace(log.RecipientPhone))
                        {
                            var fromStr = fromPhone.StartsWith("whatsapp:") ? fromPhone : $"whatsapp:{fromPhone}";
                            var toStr = log.RecipientPhone.StartsWith("whatsapp:") ? log.RecipientPhone : $"whatsapp:{log.RecipientPhone}";

                            var response = await MessageResource.CreateAsync(
                                body: log.MessageBody,
                                from: new PhoneNumber(fromStr),
                                to: new PhoneNumber(toStr)
                            );
                            if (response != null && !string.IsNullOrEmpty(response.Sid))
                            {
                                log.Status = "Sent";
                                log.ProviderMessageId = response.Sid;
                                log.SentAt = DateTime.UtcNow;
                                log.FailureReason = null;
                            }
                        }
                    }
                    else if (log.Channel.Equals("Voice", StringComparison.OrdinalIgnoreCase))
                    {
                        var fromPhone = _configuration["Twilio:FromPhone"];
                        var baseUrl = _configuration["Twilio:BaseUrl"] ?? "http://localhost:5000";
                        if (InitTwilio() && !string.IsNullOrWhiteSpace(fromPhone) && !string.IsNullOrWhiteSpace(log.RecipientPhone))
                        {
                            var callbackUrl = $"{baseUrl.TrimEnd('/')}/api/twilio/twiml/{log.Id}";
                            var call = await CallResource.CreateAsync(
                                to: new PhoneNumber(log.RecipientPhone),
                                from: new PhoneNumber(fromPhone),
                                url: new Uri(callbackUrl)
                            );
                            if (call != null && !string.IsNullOrEmpty(call.Sid))
                            {
                                log.Status = "Sent";
                                log.ProviderMessageId = call.Sid;
                                log.SentAt = DateTime.UtcNow;
                                log.FailureReason = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.FailureReason = $"Retry attempt {log.RetryCount} failed: {ex.Message}";
                    _logger.LogError(ex, $"Retry failed for NotificationLog {log.Id}");
                }
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "FAILED_NOTIFICATIONS_RETRIED",
                ResourceType = "NotificationLog",
                ResourceId = Guid.Empty.ToString(),
                TenantId = Guid.Empty
            });
        }

        public async Task<bool> RetryLogAsync(Guid logId)
        {
            var log = await _context.NotificationLogs.FindAsync(logId);
            if (log == null || !log.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            log.RetryCount++;
            bool success = false;
            try
            {
                if (log.Channel.Equals("SMS", StringComparison.OrdinalIgnoreCase))
                {
                    var fromPhone = _configuration["Twilio:FromPhone"];
                    if (InitTwilio() && !string.IsNullOrWhiteSpace(fromPhone) && !string.IsNullOrWhiteSpace(log.RecipientPhone))
                    {
                        var response = await MessageResource.CreateAsync(
                            body: log.MessageBody,
                            from: new PhoneNumber(fromPhone),
                            to: new PhoneNumber(log.RecipientPhone)
                        );
                        if (response != null && !string.IsNullOrEmpty(response.Sid))
                        {
                            log.Status = "Sent";
                            log.ProviderMessageId = response.Sid;
                            log.SentAt = DateTime.UtcNow;
                            log.FailureReason = null;
                            success = true;
                        }
                    }
                }
                else if (log.Channel.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
                {
                    var fromPhone = _configuration["Twilio:WhatsAppFromNumber"];
                    if (InitTwilio() && !string.IsNullOrWhiteSpace(fromPhone) && !string.IsNullOrWhiteSpace(log.RecipientPhone))
                    {
                        var fromStr = fromPhone.StartsWith("whatsapp:") ? fromPhone : $"whatsapp:{fromPhone}";
                        var toStr = log.RecipientPhone.StartsWith("whatsapp:") ? log.RecipientPhone : $"whatsapp:{log.RecipientPhone}";

                        var response = await MessageResource.CreateAsync(
                            body: log.MessageBody,
                            from: new PhoneNumber(fromStr),
                            to: new PhoneNumber(toStr)
                        );
                        if (response != null && !string.IsNullOrEmpty(response.Sid))
                        {
                            log.Status = "Sent";
                            log.ProviderMessageId = response.Sid;
                            log.SentAt = DateTime.UtcNow;
                            log.FailureReason = null;
                            success = true;
                        }
                    }
                }
                else if (log.Channel.Equals("Voice", StringComparison.OrdinalIgnoreCase))
                {
                    var fromPhone = _configuration["Twilio:FromPhone"];
                    var baseUrl = _configuration["Twilio:BaseUrl"] ?? "http://localhost:5000";
                    if (InitTwilio() && !string.IsNullOrWhiteSpace(fromPhone) && !string.IsNullOrWhiteSpace(log.RecipientPhone))
                    {
                        var callbackUrl = $"{baseUrl.TrimEnd('/')}/api/twilio/twiml/{log.Id}";
                        var call = await CallResource.CreateAsync(
                            to: new PhoneNumber(log.RecipientPhone),
                            from: new PhoneNumber(fromPhone),
                            url: new Uri(callbackUrl)
                        );
                        if (call != null && !string.IsNullOrEmpty(call.Sid))
                        {
                            log.Status = "Sent";
                            log.ProviderMessageId = call.Sid;
                            log.SentAt = DateTime.UtcNow;
                            log.FailureReason = null;
                            success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.FailureReason = $"Retry attempt {log.RetryCount} failed: {ex.Message}";
                _logger.LogError(ex, $"Single log retry failed for {logId}");
            }

            _context.NotificationLogs.Update(log);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "FAILED_NOTIFICATION_RETRIED",
                ResourceType = "NotificationLog",
                ResourceId = log.Id.ToString(),
                TenantId = log.TenantId
            });

            return success;
        }

        public Task SendLabReportReadyAsync(LabReportReady evt)
        {
            // TODO: Implement notifications sending (SMS/Email/InApp) for lab report ready event
            return Task.CompletedTask;
        }

        public Task SendAppointmentConfirmationAsync(AppointmentBookedEvent evt)
        {
            // TODO: Implement SMS/email confirmation using existing Twilio/template pipeline
            return Task.CompletedTask;
        }
    }
}

