using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditEvent auditEvent);
    }
}
