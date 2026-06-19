using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Modules.Appointments.Services
{
    public interface IAppointmentService
    {
        Task<List<Appointment>> GetUpcomingAsync(Guid tenantId, int days = 7);
        Task<List<DateTime>> GetAvailableSlotsAsync(Guid doctorId, DateTime date, Guid tenantId);
        Task<Appointment> BookAsync(Guid patientId, Guid doctorId, DateTime slotStart,
                                    string appointmentType, string notes,
                                    string bookedByUserId, Guid tenantId);
        Task CancelAsync(Guid appointmentId, Guid tenantId);
        Task MarkCompletedAsync(Guid appointmentId, Guid tenantId);
        Task<List<Appointment>> GetByDoctorAsync(Guid doctorId, DateTime date, Guid tenantId);
        Task<List<Appointment>> GetByPatientAsync(Guid patientId, Guid tenantId);
        Task<List<Appointment>> GetAppointmentsForRangeAsync(DateTime start, DateTime end, Guid tenantId);
    }
}
