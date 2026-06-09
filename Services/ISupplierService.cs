using CareSphere.Models;

namespace CareSphere.Services
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
