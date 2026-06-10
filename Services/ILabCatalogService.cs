using CareSphere.Models;

namespace CareSphere.Services
{
    public interface ILabCatalogService
    {
        Task<LabTestCatalog> CreateTestAsync(LabTestCatalog test);
        Task<LabTestParameter> AddParameterAsync(Guid testId, LabTestParameter parameter);
        Task<LabTestParameter> UpdateParameterAsync(LabTestParameter parameter);
        Task<LabTestCatalog?> GetTestByIdAsync(Guid id);
        Task<List<LabTestCatalog>> GetAllTestsAsync(Guid tenantId, string? search, string? category, bool? isActive);
        Task<List<LabTestCatalog>> GetTestsByCategoryAsync(Guid tenantId, string category);
    }
}
