using CareSphere.Models;

namespace CareSphere.Services
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
