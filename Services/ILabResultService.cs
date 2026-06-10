using CareSphere.Models;

namespace CareSphere.Services
{
    public interface ILabResultService
    {
        Task<LabResult> EnterResultAsync(Guid tenantId, Guid requisitionItemId, Guid parameterId, string resultValue, decimal? resultNumeric, string? notes, string enteredByUserId);
        Task VerifyResultAsync(Guid resultId, string verifiedByUserId);
        Task<List<LabResult>> GetResultsByRequisitionItemAsync(Guid tenantId, Guid requisitionItemId);
        Task<List<LabResult>> GetAbnormalResultsByPatientAsync(Guid tenantId, Guid patientId);
    }
}
