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
    public interface IEncounterService
    {
        Task<Encounter> CreateEncounterAsync(Encounter encounter);
        Task<List<Encounter>> GetEncountersByPatientAsync(Guid patientId);
        Task<Encounter?> GetEncounterByIdAsync(Guid id);
        Task UpdateEncounterStatusAsync(Guid id, string status);
        Task<Encounter?> GetActiveEncounterForPatientAsync(Guid patientId);
        Task<List<Encounter>> GetAllEncountersAsync(string? searchTerm = null, string? statusFilter = null, string? typeFilter = null);
        Task SetDispositionAsync(Guid encounterId, string disposition);
        Task AddDiagnosisAsync(Guid encounterId, string icdCode, string icdDescription, string diagnosisType);
        Task<List<EncounterDiagnosis>> GetDiagnosesAsync(Guid encounterId);
    }
}
