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

namespace CareSphere.Modules.Laboratory.Services
{
    public interface ILabResultService
    {
        Task<LabResult> EnterResultAsync(Guid tenantId, Guid requisitionItemId, Guid parameterId, string resultValue, decimal? resultNumeric, string? notes, string enteredByUserId);
        Task VerifyResultAsync(Guid resultId, string verifiedByUserId);
        Task<List<LabResult>> GetResultsByRequisitionItemAsync(Guid tenantId, Guid requisitionItemId);
        Task<List<LabResult>> GetAbnormalResultsByPatientAsync(Guid tenantId, Guid patientId);
    }
}
