using CareSphere.Models;

namespace CareSphere.Services
{
    public interface ITeleConsultService
    {
        Task<TeleConsultSession> CreateSessionAsync(TeleConsultSession session);
        Task StartSessionAsync(Guid sessionId);
        Task EndSessionAsync(Guid sessionId);
        Task<List<TeleConsultSession>> GetSessionsByEncounterAsync(Guid encounterId);
    }
}
