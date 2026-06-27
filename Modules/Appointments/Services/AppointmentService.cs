using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;
using CareSphere.Modules.Shared.Events;
using CareSphere.Modules.Clinical.Services;
using CareSphere.Infrastructure;

namespace CareSphere.Modules.Appointments.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IQueueService _queueService;
        private readonly RealTimeEventBus _eventBus;

        public AppointmentService(ApplicationDbContext dbContext, IQueueService queueService, RealTimeEventBus eventBus)
        {
            _dbContext = dbContext;
            _queueService = queueService;
            _eventBus = eventBus;
        }

        public async Task<List<Appointment>> GetUpcomingAsync(Guid tenantId, int days = 7)
        {
            var today = DateTime.UtcNow.Date;
            var until = today.AddDays(days + 1);
            return await _dbContext.Appointments.AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.TenantId == tenantId && a.SlotStart >= today && a.SlotStart < until)
                .OrderBy(a => a.SlotStart)
                .ToListAsync();
        }

        public async Task<List<DateTime>> GetAvailableSlotsAsync(Guid doctorId, DateTime date, Guid tenantId)
        {
            var dayOfWeekVal = (int)date.DayOfWeek;
            var schedule = await _dbContext.DoctorSchedules.AsNoTracking()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeekVal && s.IsActive && s.TenantId == tenantId);

            if (schedule == null)
                return new List<DateTime>();

            var slots = new List<DateTime>();
            var current = date.Date.Add(schedule.StartTime);
            var end = date.Date.Add(schedule.EndTime);
            var duration = TimeSpan.FromMinutes(schedule.SlotDurationMinutes);

            while (current + duration <= end)
            {
                slots.Add(current);
                current = current.Add(duration);
            }

            var startDate = date.Date;
            var endDate = date.Date.AddDays(1);

            var bookedSlots = await _dbContext.Appointments.AsNoTracking()
                .AsNoTracking()
                .Where(a => a.DoctorId == doctorId &&
                            a.TenantId == tenantId &&
                            a.SlotStart >= startDate &&
                            a.SlotStart < endDate &&
                            a.Status != "Cancelled")
                .Select(a => a.SlotStart)
                .ToListAsync();

            return slots.Where(s => !bookedSlots.Contains(s)).ToList();
        }

        public async Task<Appointment> BookAsync(Guid patientId, Guid doctorId, DateTime slotStart,
                                            string appointmentType, string notes,
                                            string bookedByUserId, Guid tenantId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Re-verify slot availability
                var isBooked = await _dbContext.Appointments
                    .AnyAsync(a => a.DoctorId == doctorId &&
                                   a.SlotStart == slotStart &&
                                   a.Status != "Cancelled" &&
                                   a.TenantId == tenantId);

                if (isBooked)
                {
                    throw new InvalidOperationException("Slot no longer available");
                }

                // Get slot duration to compute end time
                var dayOfWeekVal = (int)slotStart.DayOfWeek;
                var schedule = await _dbContext.DoctorSchedules
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeekVal && s.IsActive && s.TenantId == tenantId);

                var durationMinutes = schedule?.SlotDurationMinutes ?? 15;
                var slotEnd = slotStart.AddMinutes(durationMinutes);

                var appointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    PatientId = patientId,
                    DoctorId = doctorId,
                    TenantId = tenantId,
                    SlotStart = slotStart,
                    SlotEnd = slotEnd,
                    AppointmentType = appointmentType,
                    Status = "Scheduled",
                    Notes = notes,
                    BookedByUserId = bookedByUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Appointments.Add(appointment);

                // Insert into outbox
                var eventPayload = new AppointmentBookedEvent
                {
                    AppointmentId = appointment.Id,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    SlotStart = slotStart,
                    TenantId = tenantId
                };

                var outboxMessage = new ServiceBusOutbox
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    MessageType = "AppointmentBooked",
                    Payload = JsonSerializer.Serialize(eventPayload),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.ServiceBusOutboxes.Add(outboxMessage);

                // Save both in the same call
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                // If appointment is for today, auto-add to queue
                if (slotStart.Date == DateTime.UtcNow.Date)
                {
                    try
                    {
                        var queueEntry = new DoctorQueueEntry
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            DoctorId = doctorId,
                            PatientId = patientId,
                            Status = "Waiting",
                            CheckedInAt = DateTime.UtcNow,
                            TriagePriority = appointmentType.Contains("Emergency", StringComparison.OrdinalIgnoreCase) ? "Emergency" : "Routine",
                            Notes = $"From Appointment {appointment.Id}"
                        };
                        await _queueService.AddToQueueAsync(queueEntry);
                        await _eventBus.PublishAsync("QueueUpdated", doctorId.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to auto-add to queue: {ex.Message}");
                    }
                }

                return appointment;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelAsync(Guid appointmentId, Guid tenantId)
        {
            var appointment = await _dbContext.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.TenantId == tenantId);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                appointment.UpdatedAt = DateTime.UtcNow;
                _dbContext.Appointments.Update(appointment);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task MarkCompletedAsync(Guid appointmentId, Guid tenantId)
        {
            var appointment = await _dbContext.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.TenantId == tenantId);
            if (appointment != null)
            {
                appointment.Status = "Completed";
                appointment.UpdatedAt = DateTime.UtcNow;
                _dbContext.Appointments.Update(appointment);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task MarkArrivedAsync(Guid appointmentId, Guid tenantId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var appointment = await _dbContext.Appointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.TenantId == tenantId);
                if (appointment == null)
                {
                    throw new KeyNotFoundException("Appointment not found");
                }

                appointment.Status = "Arrived";
                appointment.UpdatedAt = DateTime.UtcNow;
                _dbContext.Appointments.Update(appointment);

                // Insert into outbox
                var eventPayload = new PatientArrived
                {
                    AppointmentId = appointment.Id,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    TenantId = tenantId
                };

                var outboxMessage = new ServiceBusOutbox
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    MessageType = "PatientArrived",
                    Payload = JsonSerializer.Serialize(eventPayload),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.ServiceBusOutboxes.Add(outboxMessage);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Auto-add to queue on arrival if not already there
                try
                {
                    var existingQueue = await _queueService.GetQueueForDoctorAsync(appointment.DoctorId);
                    if (!existingQueue.Any(q => q.PatientId == appointment.PatientId))
                    {
                        var queueEntry = new DoctorQueueEntry
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            DoctorId = appointment.DoctorId,
                            PatientId = appointment.PatientId,
                            Status = "Waiting",
                            CheckedInAt = DateTime.UtcNow,
                            TriagePriority = "Routine",
                            Notes = $"From Appointment {appointment.Id}"
                        };
                        await _queueService.AddToQueueAsync(queueEntry);
                        await _eventBus.PublishAsync("QueueUpdated", appointment.DoctorId.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to auto-add to queue on arrival: {ex.Message}");
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Appointment>> GetByDoctorAsync(Guid doctorId, DateTime date, Guid tenantId)
        {
            var startDate = date.Date;
            var endDate = date.Date.AddDays(1);
            return await _dbContext.Appointments.AsNoTracking()
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && a.TenantId == tenantId && a.SlotStart >= startDate && a.SlotStart < endDate)
                .OrderBy(a => a.SlotStart)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByPatientAsync(Guid patientId, Guid tenantId)
        {
            return await _dbContext.Appointments.AsNoTracking()
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId && a.TenantId == tenantId)
                .OrderByDescending(a => a.SlotStart)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsForRangeAsync(DateTime start, DateTime end, Guid tenantId)
        {
            return await _dbContext.Appointments.AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.TenantId == tenantId && a.SlotStart >= start && a.SlotStart <= end)
                .OrderBy(a => a.SlotStart)
                .ToListAsync();
        }
    }
}
