using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
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
            return await _context.StockLedgerEntries
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
            return await _context.StockLedgerEntries
                .Where(l => l.BatchId == batchId)
                .OrderByDescending(l => l.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<StockSummaryDto>> GetStockSummaryAsync(Guid tenantId, string? searchTerm = null)
        {
            var query = _context.PharmacyItems.Where(i => i.TenantId == tenantId && i.IsActive);

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
                TotalCurrentStock = _context.PharmacyBatches.Where(b => b.ItemId == i.Id && b.IsActive).Sum(b => b.CurrentStock),
                TotalAvailableStock = _context.PharmacyBatches.Where(b => b.ItemId == i.Id && b.IsActive).Sum(b => b.AvailableStock),
                BatchCount = _context.PharmacyBatches.Count(b => b.ItemId == i.Id && b.IsActive)
            })
            .OrderBy(s => s.ItemName)
            .ToListAsync();
        }
    }
}
