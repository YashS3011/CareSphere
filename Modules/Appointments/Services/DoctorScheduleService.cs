using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;

namespace CareSphere.Modules.Appointments.Services
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly ApplicationDbContext _dbContext;

        public DoctorScheduleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<DoctorSchedule>> GetScheduleAsync(Guid doctorId, Guid tenantId)
        {
            return await _dbContext.DoctorSchedules.AsNoTracking()
                .Where(s => s.DoctorId == doctorId && s.TenantId == tenantId)
                .OrderBy(s => s.DayOfWeek)
                .ToListAsync();
        }

        public async Task SetScheduleAsync(Guid doctorId, List<DoctorSchedule> schedules, Guid tenantId)
        {
            foreach (var schedule in schedules)
            {
                var existing = await _dbContext.DoctorSchedules
                    .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == schedule.DayOfWeek && s.TenantId == tenantId);

                if (existing != null)
                {
                    existing.StartTime = schedule.StartTime;
                    existing.EndTime = schedule.EndTime;
                    existing.SlotDurationMinutes = schedule.SlotDurationMinutes;
                    existing.IsActive = schedule.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;
                    _dbContext.DoctorSchedules.Update(existing);
                }
                else
                {
                    var newSchedule = new DoctorSchedule
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = doctorId,
                        TenantId = tenantId,
                        DayOfWeek = schedule.DayOfWeek,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        SlotDurationMinutes = schedule.SlotDurationMinutes,
                        IsActive = schedule.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.DoctorSchedules.Add(newSchedule);
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<DoctorSchedule> UpdateSlotDurationAsync(Guid scheduleId, int minutes, Guid tenantId)
        {
            var schedule = await _dbContext.DoctorSchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.TenantId == tenantId);
            if (schedule == null)
            {
                throw new InvalidOperationException("Schedule not found.");
            }

            schedule.SlotDurationMinutes = minutes;
            schedule.UpdatedAt = DateTime.UtcNow;
            _dbContext.DoctorSchedules.Update(schedule);
            await _dbContext.SaveChangesAsync();
            return schedule;
        }
    }
}
