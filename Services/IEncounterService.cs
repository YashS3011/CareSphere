using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IEncounterService
    {
        Task<Encounter> CreateEncounterAsync(Encounter encounter);
        Task<List<Encounter>> GetEncountersByPatientAsync(Guid patientId);
        Task<Encounter?> GetEncounterByIdAsync(Guid id);
        Task UpdateEncounterStatusAsync(Guid id, string status);
        Task<Encounter?> GetActiveEncounterForPatientAsync(Guid patientId);
        Task<List<Encounter>> GetAllEncountersAsync(string? searchTerm = null, string? statusFilter = null, string? typeFilter = null);
    }
}
