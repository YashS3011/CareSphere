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
    public interface IGrnService
    {
        Task<GoodsReceivedNote> CreateGrnAsync(GoodsReceivedNote grn);
        Task<GoodsReceivedNote?> GetGrnByIdAsync(Guid id);
        Task<List<GoodsReceivedNote>> GetGrnsAsync(Guid tenantId, Guid? supplierId, string? status, int page = 1, int pageSize = 10);
        Task<int> GetGrnsCountAsync(Guid tenantId, Guid? supplierId, string? status);
        Task<GoodsReceivedNote> PostGrnAsync(Guid grnId);
        Task<GoodsReceivedNote> UpdateGrnAsync(GoodsReceivedNote grn);
    }
}
