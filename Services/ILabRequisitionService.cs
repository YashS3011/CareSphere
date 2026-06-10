using CareSphere.Models;

namespace CareSphere.Services
{
    public interface ILabRequisitionService
    {
        Task<LabRequisition> CreateRequisitionAsync(Guid tenantId, Guid patientId, Guid? encounterId, Guid doctorId, string priority, string? clinicalNotes, List<Guid> testIds);
        Task<LabRequisition?> GetRequisitionByIdAsync(Guid id);
        Task<List<LabRequisition>> GetRequisitionsByPatientAsync(Guid tenantId, Guid patientId);
        Task<List<LabRequisition>> GetRequisitionsByEncounterAsync(Guid tenantId, Guid encounterId);
        Task<List<LabRequisition>> GetPendingRequisitionsAsync(Guid tenantId);
        Task UpdateRequisitionStatusAsync(Guid requisitionId, string status);
        Task CancelRequisitionAsync(Guid requisitionId);
    }
}
