using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Modules.Analytics.Models;

namespace CareSphere.Modules.Analytics.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _dbContext;

        public AnalyticsService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BedOccupancyMetrics> GetBedOccupancyAsync(Guid tenantId)
        {
            var totalBeds = await _dbContext.Beds
                .AsNoTracking()
                .CountAsync(b => b.TenantId == tenantId && b.IsActive);

            var occupiedBeds = await _dbContext.BedAllotments
                .AsNoTracking()
                .CountAsync(a => a.TenantId == tenantId && a.Status == "Active");

            var dischargedAllotments = await _dbContext.BedAllotments.AsNoTracking()
                .AsNoTracking()
                .Where(a => a.TenantId == tenantId && a.Status == "Discharged" && a.DischargeDate != null)
                .Select(a => new { a.AdmissionDate, a.DischargeDate })
                .ToListAsync();

            double avgStay = 0;
            if (dischargedAllotments.Any())
            {
                avgStay = dischargedAllotments.Average(a => (a.DischargeDate!.Value - a.AdmissionDate).TotalDays);
            }

            decimal occupancyRate = 0;
            if (totalBeds > 0)
            {
                occupancyRate = ((decimal)occupiedBeds / totalBeds) * 100;
            }

            return new BedOccupancyMetrics
            {
                TotalBeds = totalBeds,
                OccupiedBeds = occupiedBeds,
                AvailableBeds = Math.Max(0, totalBeds - occupiedBeds),
                OccupancyRatePercent = occupancyRate,
                AverageLengthOfStayDays = avgStay
            };
        }

        private static DateTime NormalizeToUtc(DateTime dt, bool endOfDay = false)
        {
            var utc = dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };
            return endOfDay ? utc.Date.AddDays(1).AddTicks(-1) : utc.Date;
        }

        public async Task<RevenueMetrics> GetRevenueAsync(Guid tenantId, DateTime from, DateTime to)
        {
            var fromUtc = NormalizeToUtc(from, false);
            var toUtc = NormalizeToUtc(to, true);

            var invoices = await _dbContext.BillingInvoices.AsNoTracking()
                .AsNoTracking()
                .Where(i => i.TenantId == tenantId && i.CreatedAt >= fromUtc && i.CreatedAt <= toUtc)
                .ToListAsync();

            return new RevenueMetrics
            {
                TotalInvoiced = invoices.Sum(i => i.TotalAmount),
                TotalCollected = invoices.Sum(i => i.PaidAmount),
                TotalOutstanding = invoices.Sum(i => i.BalanceAmount),
                InvoiceCount = invoices.Count
            };
        }

        public async Task<LabMetrics> GetLabMetricsAsync(Guid tenantId)
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var totalRequisitions = await _dbContext.LabRequisitions
                .AsNoTracking()
                .CountAsync(r => r.TenantId == tenantId && r.OrderedAt >= firstDayOfMonth);

            var abnormalResults = await _dbContext.LabResults
                .AsNoTracking()
                .CountAsync(r => r.TenantId == tenantId && r.IsAbnormal && r.EnteredAt >= firstDayOfMonth);

            var completedRequisitions = await _dbContext.LabRequisitions.AsNoTracking()
                .AsNoTracking()
                .Where(r => r.TenantId == tenantId && r.Status == "Completed" && r.OrderedAt >= firstDayOfMonth)
                .Include(r => r.Items)
                .ThenInclude(ri => ri.Results)
                .ToListAsync();

            double avgTurnaround = 0;
            var diffs = new List<double>();
            foreach (var req in completedRequisitions)
            {
                var results = req.Items.SelectMany(i => i.Results).Where(res => res.VerifiedAt != null).ToList();
                if (results.Any())
                {
                    var maxVerifiedAt = results.Max(res => res.VerifiedAt!.Value);
                    diffs.Add((maxVerifiedAt - req.OrderedAt).TotalHours);
                }
            }
            if (diffs.Any())
            {
                avgTurnaround = diffs.Average();
            }

            return new LabMetrics
            {
                TotalRequisitionsThisMonth = totalRequisitions,
                AbnormalResultsThisMonth = abnormalResults,
                AverageTurnaroundHours = avgTurnaround
            };
        }

        public async Task<TopDoctorsMetrics> GetTopDoctorsByEncountersAsync(Guid tenantId, int topN = 5)
        {
            var doctorCounts = await _dbContext.Encounters.AsNoTracking()
                .AsNoTracking()
                .Where(e => e.TenantId == tenantId)
                .GroupBy(e => e.DoctorId)
                .Select(g => new { DoctorId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topN)
                .ToListAsync();

            var doctorIds = doctorCounts.Select(x => x.DoctorId).ToList();

            var doctors = await _dbContext.Doctors.AsNoTracking()
                .AsNoTracking()
                .Where(d => doctorIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id);

            var topDoctorsList = doctorCounts
                .Where(dc => doctors.ContainsKey(dc.DoctorId))
                .Select(dc => new DoctorEncounterCount
                {
                    DoctorName = doctors[dc.DoctorId].FullName,
                    Specialty = doctors[dc.DoctorId].Specialization,
                    EncounterCount = dc.Count
                }).ToList();

            return new TopDoctorsMetrics
            {
                TopDoctors = topDoctorsList
            };
        }

        public async Task<PharmacyMetrics> GetPharmacyMetricsAsync(Guid tenantId)
        {
            var batches = await _dbContext.PharmacyBatches.AsNoTracking()
                .AsNoTracking()
                .Where(b => b.TenantId == tenantId && b.IsActive)
                .ToListAsync();

            var stockValue = batches.Sum(b => b.CurrentStock * b.PurchasePrice);
            var nearExpiryCount = batches.Count(b => b.ExpiryDate <= DateTime.UtcNow.AddDays(90));

            var items = await _dbContext.PharmacyItems.AsNoTracking()
                .AsNoTracking()
                .Where(i => i.TenantId == tenantId && i.IsActive)
                .ToListAsync();

            var itemStockSums = batches
                .GroupBy(b => b.ItemId)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.CurrentStock));

            int lowStockCount = 0;
            foreach (var item in items)
            {
                var stock = itemStockSums.TryGetValue(item.Id, out var s) ? s : 0;
                if (stock <= item.ReorderLevel)
                {
                    lowStockCount++;
                }
            }

            return new PharmacyMetrics
            {
                TotalStockValue = stockValue,
                NearExpiryBatchesCount = nearExpiryCount,
                LowStockItemsCount = lowStockCount
            };
        }
    }
}
