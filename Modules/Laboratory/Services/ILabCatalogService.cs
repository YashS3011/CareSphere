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
