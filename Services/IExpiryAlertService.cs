using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IExpiryAlertService
    {
        Task CheckAndGenerateExpiryAlertsAsync(Guid tenantId);
        Task<List<ExpiryAlert>> GetUnacknowledgedAlertsAsync(Guid tenantId);
        Task<List<ExpiryAlert>> GetAllAlertsAsync(Guid tenantId);
        Task AcknowledgeAlertAsync(Guid alertId, string acknowledgedByUserId);
    }
}
