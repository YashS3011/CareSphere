using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IPharmacyItemService
    {
        Task<PharmacyItem> CreateItemAsync(PharmacyItem item);
        Task<PharmacyItem?> GetItemByIdAsync(Guid id);
        Task<PharmacyItem?> GetItemByBarcodeAsync(Guid tenantId, string barcode);
        Task<List<PharmacyItem>> SearchItemsAsync(Guid tenantId, string? query, string? category, bool? requiresPrescription, int page = 1, int pageSize = 10);
        Task<int> GetItemsCountAsync(Guid tenantId, string? query, string? category, bool? requiresPrescription);
        Task<PharmacyItem> UpdateItemAsync(PharmacyItem item);
        Task<List<PharmacyItemStockDto>> GetLowStockItemsAsync(Guid tenantId);
        Task<List<PharmacyItem>> GetActiveItemsAsync(Guid tenantId);
    }

    public class PharmacyItemStockDto
    {
        public PharmacyItem Item { get; set; } = null!;
        public int TotalStock { get; set; }
    }
}
