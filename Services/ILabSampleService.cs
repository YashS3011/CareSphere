using CareSphere.Models;

namespace CareSphere.Services
{
    public interface ILabSampleService
    {
        Task<LabSample> RecordSampleCollectionAsync(Guid tenantId, Guid requisitionId, string sampleType, string collectedByUserId, string? barcodeLabel, string? notes, DateTime collectedAt);
        Task ReceiveSampleAsync(Guid sampleId, string receivedByUserId);
        Task<List<LabSample>> GetSamplesByRequisitionAsync(Guid tenantId, Guid requisitionId);
    }
}
