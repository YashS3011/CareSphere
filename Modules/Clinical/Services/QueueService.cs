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

namespace CareSphere.Modules.Clinical.Services
{
    public class QueueService : IQueueService
    {
        private readonly ApplicationDbContext _context;

        public QueueService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DoctorQueueEntry> AddToQueueAsync(DoctorQueueEntry entry)
        {
            entry.Id = Guid.NewGuid();
            entry.CheckedInAt = DateTime.UtcNow;
            entry.Status = "Waiting";

            // Calculate next position for this doctor's queue
            var maxPosition = await _context.DoctorQueueEntries
                .Where(q => q.DoctorId == entry.DoctorId && (q.Status == "Waiting" || q.Status == "InConsultation"))
                .MaxAsync(q => (int?)q.QueuePosition) ?? 0;

            entry.QueuePosition = maxPosition + 1;

            // Calculate estimated wait
            var avgMinutes = await CalculateEtaAsync(entry.DoctorId);
            var waitingCount = await _context.DoctorQueueEntries
                .CountAsync(q => q.DoctorId == entry.DoctorId && q.Status == "Waiting");
            entry.EstimatedWaitMinutes = avgMinutes * waitingCount;

            _context.DoctorQueueEntries.Add(entry);
            await _context.SaveChangesAsync();

            return entry;
        }

        public async Task UpdateStatusAsync(Guid entryId, string newStatus)
        {
            var entry = await _context.DoctorQueueEntries.FindAsync(entryId);
            if (entry == null)
                throw new InvalidOperationException("Queue entry not found.");

            entry.Status = newStatus;

            switch (newStatus)
            {
                case "InConsultation":
                    entry.StartedAt = DateTime.UtcNow;
                    break;
                case "Completed":
                    entry.CompletedAt = DateTime.UtcNow;
                    break;
                case "NoShow":
                    entry.CompletedAt = DateTime.UtcNow;
                    break;
            }

            _context.DoctorQueueEntries.Update(entry);

            // Recalculate positions and ETAs for remaining waiting patients
            if (newStatus == "Completed" || newStatus == "NoShow")
            {
                await RecalculateQueueAsync(entry.DoctorId);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<DoctorQueueEntry>> GetQueueForDoctorAsync(Guid doctorId)
        {
            return await _context.DoctorQueueEntries.AsNoTracking()
                .Include(q => q.Patient)
                .Include(q => q.Doctor)
                .Where(q => q.DoctorId == doctorId &&
                            (q.Status == "Waiting" || q.Status == "InConsultation"))
                .OrderBy(q => q.TriagePriority == "Emergency" ? 1 :
                             q.TriagePriority == "Urgent" ? 2 : 3)
                .ThenBy(q => q.QueuePosition)
                .ToListAsync();
        }

        public async Task<int> CalculateEtaAsync(Guid doctorId)
        {
            // Average the last 10 completed consultation durations for this doctor
            var completedEntries = await _context.DoctorQueueEntries
                .Where(q => q.DoctorId == doctorId && q.Status == "Completed" && q.StartedAt != null && q.CompletedAt != null)
                .OrderByDescending(q => q.CompletedAt)
                .Take(10)
                .ToListAsync();

            if (!completedEntries.Any())
                return 10; // Default to 10 minutes if no history

            var avgMinutes = completedEntries
                .Average(q => (q.CompletedAt!.Value - q.StartedAt!.Value).TotalMinutes);

            return (int)Math.Round(avgMinutes);
        }

        private async Task RecalculateQueueAsync(Guid doctorId)
        {
            var waitingEntries = await _context.DoctorQueueEntries
                .Where(q => q.DoctorId == doctorId && q.Status == "Waiting")
                .OrderBy(q => q.TriagePriority == "Emergency" ? 1 :
                             q.TriagePriority == "Urgent" ? 2 : 3)
                .ThenBy(q => q.QueuePosition)
                .ToListAsync();

            var avgMinutes = await CalculateEtaAsync(doctorId);

            for (int i = 0; i < waitingEntries.Count; i++)
            {
                waitingEntries[i].QueuePosition = i + 1;
                waitingEntries[i].EstimatedWaitMinutes = avgMinutes * (i + 1);
                _context.DoctorQueueEntries.Update(waitingEntries[i]);
            }
        }
    }
}
