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
using CareSphere.Models;

namespace CareSphere.Modules.Pharmacy.Services
{
    public interface IExpiryAlertService
    {
        Task CheckAndGenerateExpiryAlertsAsync(Guid tenantId);
        Task<List<ExpiryAlert>> GetUnacknowledgedAlertsAsync(Guid tenantId);
        Task<List<ExpiryAlert>> GetAllAlertsAsync(Guid tenantId);
        Task AcknowledgeAlertAsync(Guid alertId, string acknowledgedByUserId);
    }
}
