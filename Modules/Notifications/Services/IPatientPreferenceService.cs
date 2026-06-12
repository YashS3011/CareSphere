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
using System;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Modules.Notifications.Services
{
    public interface IPatientPreferenceService
    {
        Task<PatientPreference> GetOrCreatePreferencesAsync(Guid tenantId, Guid patientId);
        Task<PatientPreference> UpdatePreferencesAsync(PatientPreference preferences);
    }
}
