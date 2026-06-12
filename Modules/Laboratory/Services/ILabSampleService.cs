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
    public interface ILabSampleService
    {
        Task<LabSample> RecordSampleCollectionAsync(Guid tenantId, Guid requisitionId, string sampleType, string collectedByUserId, string? barcodeLabel, string? notes, DateTime collectedAt);
        Task ReceiveSampleAsync(Guid sampleId, string receivedByUserId);
        Task<List<LabSample>> GetSamplesByRequisitionAsync(Guid tenantId, Guid requisitionId);
    }
}
