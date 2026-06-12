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
    public interface IStockLedgerService
    {
        Task<List<StockLedgerEntry>> GetLedgerByItemAsync(Guid itemId, int page = 1, int pageSize = 20);
        Task<int> GetLedgerCountByItemAsync(Guid itemId);
        Task<List<StockLedgerEntry>> GetLedgerByBatchAsync(Guid batchId);
        Task<List<StockSummaryDto>> GetStockSummaryAsync(Guid tenantId, string? searchTerm = null);
    }

    public class StockSummaryDto
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int TotalCurrentStock { get; set; }
        public int TotalAvailableStock { get; set; }
        public int BatchCount { get; set; }
    }
}
