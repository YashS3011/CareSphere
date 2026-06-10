using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IAppointmentReminderService
    {
        Task ScheduleAppointmentReminderAsync(Guid tenantId, Guid patientId, Guid? doctorId, DateTime appointmentDate, Guid? appointmentId = null, string language = "en");
        Task ScheduleFollowUpReminderAsync(Guid tenantId, Guid patientId, Guid? doctorId, DateTime followUpDate, string language = "en");
        Task ProcessAppointmentReminderAsync(Guid reminderId);
        Task CancelReminderAsync(Guid appointmentId);
        Task<List<AppointmentReminder>> GetScheduledRemindersAsync(Guid tenantId);
        
        // This method can be called from AppointmentService.CreateAppointmentAsync when the Appointments module is built.
        Task ScheduleReminderForAppointmentAsync(Guid tenantId, Guid patientId, Guid? doctorId, DateTime appointmentDate, Guid? appointmentId = null, string language = "en");
    }
}
