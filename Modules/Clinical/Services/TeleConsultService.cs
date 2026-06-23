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
    public class TeleConsultService : ITeleConsultService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public TeleConsultService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<TeleConsultSession> CreateSessionAsync(TeleConsultSession session)
        {
            session.Id = Guid.NewGuid();
            session.Status = "Scheduled";
            // Placeholder meeting link until LiveKit is integrated
            session.MeetingLink = $"/teleconsult/join/{session.Id}";

            _context.TeleConsultSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task StartSessionAsync(Guid sessionId)
        {
            var session = await _context.TeleConsultSessions.FindAsync(sessionId);
            if (session == null)
                throw new InvalidOperationException("Tele-consult session not found.");

            session.Status = "Active";
            session.StartedAt = DateTime.UtcNow;

            _context.TeleConsultSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        public async Task EndSessionAsync(Guid sessionId)
        {
            var session = await _context.TeleConsultSessions.FindAsync(sessionId);
            if (session == null)
                throw new InvalidOperationException("Tele-consult session not found.");

            session.Status = "Ended";
            session.EndedAt = DateTime.UtcNow;

            // Calculate duration
            if (session.StartedAt.HasValue)
            {
                session.DurationMinutes = (int)Math.Round((session.EndedAt.Value - session.StartedAt.Value).TotalMinutes);
            }

            _context.TeleConsultSessions.Update(session);
            await _context.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system", // Will be replaced once auth is added
                Action = "TELECONSULT_ENDED",
                ResourceType = "TeleConsultSession",
                ResourceId = sessionId.ToString(),
                TenantId = session.TenantId
            });
        }

        public async Task<List<TeleConsultSession>> GetSessionsByEncounterAsync(Guid encounterId)
        {
            return await _context.TeleConsultSessions.AsNoTracking()
                .Include(s => s.Doctor)
                .Include(s => s.Patient)
                .Where(s => s.EncounterId == encounterId)
                .OrderByDescending(s => s.StartedAt ?? DateTime.MinValue)
                .ToListAsync();
        }
    }
}
