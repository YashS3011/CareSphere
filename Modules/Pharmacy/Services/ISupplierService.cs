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

namespace CareSphere.Modules.Pharmacy.Services
{
    public interface ISupplierService
    {
        Task<Supplier> CreateSupplierAsync(Supplier supplier);
        Task<Supplier?> GetSupplierByIdAsync(Guid id);
        Task<List<Supplier>> GetSuppliersAsync(Guid tenantId, string? searchTerm = null);
        Task<Supplier> UpdateSupplierAsync(Supplier supplier);
        Task ToggleActiveAsync(Guid id);
    }
}
