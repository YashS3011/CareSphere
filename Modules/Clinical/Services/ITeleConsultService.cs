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

namespace CareSphere.Modules.Clinical.Services
{
    public interface ITeleConsultService
    {
        Task<TeleConsultSession> CreateSessionAsync(TeleConsultSession session);
        Task StartSessionAsync(Guid sessionId);
        Task EndSessionAsync(Guid sessionId);
        Task<List<TeleConsultSession>> GetSessionsByEncounterAsync(Guid encounterId);
    }
}
