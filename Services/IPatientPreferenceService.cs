using System;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IPatientPreferenceService
    {
        Task<PatientPreference> GetOrCreatePreferencesAsync(Guid tenantId, Guid patientId);
        Task<PatientPreference> UpdatePreferencesAsync(PatientPreference preferences);
    }
}
