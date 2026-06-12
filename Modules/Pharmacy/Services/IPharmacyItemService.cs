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
