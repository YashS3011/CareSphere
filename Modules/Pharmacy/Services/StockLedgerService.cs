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
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Modules.Pharmacy.Services
{
    public class StockLedgerService : IStockLedgerService
    {
        private readonly ApplicationDbContext _context;

        public StockLedgerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StockLedgerEntry>> GetLedgerByItemAsync(Guid itemId, int page = 1, int pageSize = 20)
        {
            return await _context.StockLedgerEntries.AsNoTracking()
                .Include(l => l.Batch)
                .Where(l => l.ItemId == itemId)
                .OrderByDescending(l => l.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetLedgerCountByItemAsync(Guid itemId)
        {
            return await _context.StockLedgerEntries
                .Where(l => l.ItemId == itemId)
                .CountAsync();
        }

        public async Task<List<StockLedgerEntry>> GetLedgerByBatchAsync(Guid batchId)
        {
            return await _context.StockLedgerEntries.AsNoTracking()
                .Where(l => l.BatchId == batchId)
                .OrderByDescending(l => l.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<StockSummaryDto>> GetStockSummaryAsync(Guid tenantId, string? searchTerm = null)
        {
            var query = _context.PharmacyItems.AsNoTracking().Where(i => i.TenantId == tenantId && i.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(i => i.ItemName.ToLower().Contains(lower) || 
                                         i.ItemCode.ToLower().Contains(lower) || 
                                         i.Category.ToLower().Contains(lower));
            }

            return await query.Select(i => new StockSummaryDto
            {
                ItemId = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.ItemName,
                Category = i.Category,
                Unit = i.Unit,
                TotalCurrentStock = _context.PharmacyBatches.AsNoTracking().Where(b => b.ItemId == i.Id && b.IsActive).Sum(b => b.CurrentStock),
                TotalAvailableStock = _context.PharmacyBatches.AsNoTracking().Where(b => b.ItemId == i.Id && b.IsActive).Sum(b => b.AvailableStock),
                BatchCount = _context.PharmacyBatches.AsNoTracking().Count(b => b.ItemId == i.Id && b.IsActive)
            })
            .OrderBy(s => s.ItemName)
            .ToListAsync();
        }
    }
}
