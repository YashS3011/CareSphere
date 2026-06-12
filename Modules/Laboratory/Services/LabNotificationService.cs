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
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace CareSphere.Modules.Laboratory.Services
{
    public class LabNotificationService : ILabNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public LabNotificationService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task SendReportNotificationsAsync(Guid tenantId, Guid labReportId)
        {
            var report = await _context.LabReports
                .Include(r => r.Requisition)
                    .ThenInclude(req => req.Patient)
                .Include(r => r.Requisition)
                    .ThenInclude(req => req.OrderedByDoctor)
                .FirstOrDefaultAsync(r => r.Id == labReportId);

            if (report == null)
            {
                throw new KeyNotFoundException("Lab report not found.");
            }

            var patientName = $"{report.Requisition.Patient.FirstName} {report.Requisition.Patient.LastName}";
            var doctorIdStr = report.Requisition.OrderedByDoctorId.ToString();
            var patientIdStr = report.Requisition.PatientId.ToString();
            var reqNumber = report.Requisition.RequisitionNumber;

            // 1. Create In-App Notification for Doctor
            var doctorNotification = new InAppNotification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RecipientType = "Doctor",
                RecipientId = doctorIdStr,
                Title = "Lab Report Ready",
                Message = $"Lab report for patient {patientName} requisition {reqNumber} is ready",
                ResourceType = "LabReport",
                ResourceId = labReportId.ToString(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.InAppNotifications.Add(doctorNotification);

            // 2. Create In-App Notification for Patient
            var patientNotification = new InAppNotification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RecipientType = "Patient",
                RecipientId = patientIdStr,
                Title = "Your Lab Report is Ready",
                Message = $"Your lab report {reqNumber} is ready. Please contact your doctor.",
                ResourceType = "LabReport",
                ResourceId = labReportId.ToString(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.InAppNotifications.Add(patientNotification);

            // 3. Configure Twilio
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromPhone = _configuration["Twilio:FromPhone"];

            var isTwilioConfigured = !string.IsNullOrWhiteSpace(accountSid) &&
                                     !string.IsNullOrWhiteSpace(authToken) &&
                                     !string.IsNullOrWhiteSpace(fromPhone);

            if (isTwilioConfigured)
            {
                try
                {
                    TwilioClient.Init(accountSid, authToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LabNotificationService] Twilio initialization failed: {ex.Message}");
                    isTwilioConfigured = false;
                }
            }

            // 4. Send SMS to Patient
            if (!string.IsNullOrWhiteSpace(report.Requisition.Patient.Phone) && isTwilioConfigured)
            {
                try
                {
                    var msg = await MessageResource.CreateAsync(
                        body: $"CareSphere: Your lab report {reqNumber} is ready. Please visit the hospital or contact your doctor.",
                        from: new PhoneNumber(fromPhone),
                        to: new PhoneNumber(report.Requisition.Patient.Phone)
                    );

                    report.PatientSmsStatus = (msg != null && !string.IsNullOrEmpty(msg.Sid)) ? "Sent" : "Failed";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LabNotificationService] Patient SMS failed: {ex.Message}");
                    report.PatientSmsStatus = "Failed";
                }
            }
            else
            {
                report.PatientSmsStatus = "Skipped";
            }

            // 5. Send SMS to Doctor
            if (!string.IsNullOrWhiteSpace(report.Requisition.OrderedByDoctor.Phone) && isTwilioConfigured)
            {
                try
                {
                    var msg = await MessageResource.CreateAsync(
                        body: $"CareSphere: Lab report ready for patient {patientName} MRN {report.Requisition.Patient.Mrn} Req {reqNumber}. Please review.",
                        from: new PhoneNumber(fromPhone),
                        to: new PhoneNumber(report.Requisition.OrderedByDoctor.Phone)
                    );

                    report.DoctorSmsStatus = (msg != null && !string.IsNullOrEmpty(msg.Sid)) ? "Sent" : "Failed";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LabNotificationService] Doctor SMS failed: {ex.Message}");
                    report.DoctorSmsStatus = "Failed";
                }
            }
            else
            {
                report.DoctorSmsStatus = "Skipped";
            }

            report.InAppNotificationSent = true;
            report.NotificationSentAt = DateTime.UtcNow;

            _context.LabReports.Update(report);
            await _context.SaveChangesAsync();
        }

        public async Task<List<InAppNotification>> GetUnreadNotificationsAsync(string recipientType, string recipientId)
        {
            return await _context.InAppNotifications
                .Where(n => n.RecipientType == recipientType && n.RecipientId == recipientId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkNotificationReadAsync(Guid notificationId)
        {
            var notification = await _context.InAppNotifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _context.InAppNotifications.Update(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetNotificationCountAsync(string recipientType, string recipientId)
        {
            return await _context.InAppNotifications
                .CountAsync(n => n.RecipientType == recipientType && n.RecipientId == recipientId && !n.IsRead);
        }
    }
}
