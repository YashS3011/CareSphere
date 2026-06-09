using CareSphere.Models;

namespace CareSphere.Services
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
