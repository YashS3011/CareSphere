using System;
using System.Threading.Tasks;
using CareSphere.Modules.Analytics.Models;

namespace CareSphere.Modules.Analytics.Services
{
    public interface IAnalyticsService
    {
        Task<BedOccupancyMetrics> GetBedOccupancyAsync(Guid tenantId);
        Task<RevenueMetrics> GetRevenueAsync(Guid tenantId, DateTime from, DateTime to);
        Task<LabMetrics> GetLabMetricsAsync(Guid tenantId);
        Task<TopDoctorsMetrics> GetTopDoctorsByEncountersAsync(Guid tenantId, int topN = 5);
        Task<PharmacyMetrics> GetPharmacyMetricsAsync(Guid tenantId);
    }
}
