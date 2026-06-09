using CareSphere.Data;
using CareSphere.Models;

namespace CareSphere.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(AuditEvent auditEvent)
        {
            auditEvent.Id = Guid.NewGuid();
            auditEvent.Timestamp = DateTime.UtcNow;

            _context.AuditEvents.Add(auditEvent);
            await _context.SaveChangesAsync();
        }
    }
}
