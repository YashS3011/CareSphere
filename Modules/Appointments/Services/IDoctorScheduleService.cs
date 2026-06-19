using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Modules.Appointments.Services
{
    public interface IDoctorScheduleService
    {
        Task<List<DoctorSchedule>> GetScheduleAsync(Guid doctorId, Guid tenantId);
        Task SetScheduleAsync(Guid doctorId, List<DoctorSchedule> schedules, Guid tenantId);
        Task<DoctorSchedule> UpdateSlotDurationAsync(Guid scheduleId, int minutes, Guid tenantId);
    }
}
