using CareSphere.Models;

namespace CareSphere.Services
{
    public interface ILabReportService
    {
        Task<LabReport> GenerateReportAsync(Guid tenantId, Guid requisitionId);
        Task<List<LabReport>> GetReportsByRequisitionAsync(Guid requisitionId);
    }
}
