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
    public interface IPurchaseOrderService
    {
        Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder po);
        Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(Guid id);
        Task<List<PurchaseOrder>> GetPurchaseOrdersAsync(Guid tenantId, string? status, Guid? supplierId, int page = 1, int pageSize = 10);
        Task<int> GetPurchaseOrdersCountAsync(Guid tenantId, string? status, Guid? supplierId);
        Task<PurchaseOrder> UpdatePurchaseOrderAsync(PurchaseOrder po);
        Task<PurchaseOrder> SendPurchaseOrderAsync(Guid poId);
        Task CancelPurchaseOrderAsync(Guid poId, string reason);
    }
}
