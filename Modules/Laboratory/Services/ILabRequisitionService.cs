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
