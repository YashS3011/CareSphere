using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Infrastructure;
using CareSphere.Models;
using CareSphere.Modules.Analytics.Services;
using Xunit;

namespace CareSphere.Tests
{
    public class AnalyticsTests
    {
        [Fact]
        public async Task GetBedOccupancyAsync_ShouldCalculateCorrectMetrics()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("CareSphere_AnalyticsTests_DB_" + Guid.NewGuid())
                .Options;

            var tenantId = Guid.NewGuid();

            using (var context = new ApplicationDbContext(options, new BypassTenantContext(tenantId)))
            {
                // Seed Wards and Beds
                var ward = new Ward
                {
                    Id = Guid.NewGuid(),
                    Name = "General Ward",
                    WardType = "General",
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };

                var bed1 = new Bed
                {
                    Id = Guid.NewGuid(),
                    BedNumber = "G-01",
                    WardId = ward.Id,
                    DailyChargeAmount = 500,
                    IsActive = true,
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };

                var bed2 = new Bed
                {
                    Id = Guid.NewGuid(),
                    BedNumber = "G-02",
                    WardId = ward.Id,
                    DailyChargeAmount = 500,
                    IsActive = true,
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };

                context.Wards.Add(ward);
                context.Beds.Add(bed1);
                context.Beds.Add(bed2);

                // Seed Bed Allotments (one active, one discharged)
                var patient1 = new Patient
                {
                    Id = Guid.NewGuid(),
                    Mrn = "MRN-AN-01",
                    FirstName = "John",
                    LastName = "Doe",
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };

                var patient2 = new Patient
                {
                    Id = Guid.NewGuid(),
                    Mrn = "MRN-AN-02",
                    FirstName = "Jane",
                    LastName = "Doe",
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };

                context.Patients.Add(patient1);
                context.Patients.Add(patient2);

                var allotment1 = new BedAllotment
                {
                    Id = Guid.NewGuid(),
                    PatientId = patient1.Id,
                    BedId = bed1.Id,
                    AdmissionDate = DateTime.UtcNow.AddDays(-2),
                    Status = "Active",
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };

                var allotment2 = new BedAllotment
                {
                    Id = Guid.NewGuid(),
                    PatientId = patient2.Id,
                    BedId = bed2.Id,
                    AdmissionDate = DateTime.UtcNow.AddDays(-5),
                    DischargeDate = DateTime.UtcNow.AddDays(-1),
                    Status = "Discharged",
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };

                context.BedAllotments.Add(allotment1);
                context.BedAllotments.Add(allotment2);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options, new BypassTenantContext(tenantId)))
            {
                var service = new AnalyticsService(context);

                // Act
                var metrics = await service.GetBedOccupancyAsync(tenantId);

                // Assert
                Assert.NotNull(metrics);
                Assert.Equal(2, metrics.TotalBeds);
                Assert.Equal(1, metrics.OccupiedBeds);
                Assert.Equal(1, metrics.AvailableBeds);
                Assert.Equal(50, metrics.OccupancyRatePercent);
                Assert.Equal(4.0, metrics.AverageLengthOfStayDays, 1);
            }
        }
    }
}
