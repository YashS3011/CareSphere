using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Infrastructure;
using CareSphere.Models;
using CareSphere.Modules.Nursing.Services;
using Xunit;

namespace CareSphere.Tests
{
    public class NursingTests
    {
        [Fact]
        public void VitalSignsThresholdService_ShouldDetectBreaches()
        {
            // Arrange
            var thresholdService = new VitalThresholdService();

            var normalVitals = new VitalSigns
            {
                SpO2 = 98,
                HeartRate = 72,
                BloodPressureSystolic = 120,
                BloodPressureDiastolic = 80,
                Temperature = 36.6m
            };

            var criticalVitals = new VitalSigns
            {
                SpO2 = 90, // < 92 critical
                HeartRate = 135, // > 130 critical
                BloodPressureSystolic = 190, // > 180 critical
                Temperature = 40.0m // > 39.5 critical
            };

            // Act
            var normalBreaches = thresholdService.CheckThresholds(normalVitals);
            var criticalBreaches = thresholdService.CheckThresholds(criticalVitals);

            // Assert
            Assert.Empty(normalBreaches);
            Assert.Equal(4, criticalBreaches.Count);
            Assert.Contains(criticalBreaches, b => b.Contains("SpO2"));
            Assert.Contains(criticalBreaches, b => b.Contains("Heart Rate"));
            Assert.Contains(criticalBreaches, b => b.Contains("Systolic BP"));
            Assert.Contains(criticalBreaches, b => b.Contains("Temperature"));
        }

        [Fact]
        public async Task ShiftHandoverService_ShouldSaveAndRetrieveHandovers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("CareSphere_NursingTests_DB_" + Guid.NewGuid())
                .Options;

            var tenantId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            using (var context = new ApplicationDbContext(options, new BypassTenantContext(tenantId)))
            {
                // Seed database with patient
                var patient = new Patient
                {
                    Id = patientId,
                    Mrn = "MRN-N001",
                    FirstName = "Jane",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1995, 8, 15),
                    Gender = "Female",
                    Phone = "5551234",
                    Email = "jane.doe@test.com",
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };
                context.Patients.Add(patient);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options, new BypassTenantContext(tenantId)))
            {
                var handoverService = new ShiftHandoverService(context);

                var handover = new ShiftHandover
                {
                    PatientId = patientId,
                    OutgoingNurseId = "nurse1",
                    IncomingNurseId = "nurse2",
                    HandoverNotes = "Stable patient. IV line running.",
                    Shift = "Morning",
                    HandoverDate = DateTime.UtcNow
                };

                // Act
                var result = await handoverService.AddHandoverAsync(handover, tenantId);

                // Assert
                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(tenantId, result.TenantId);
            }

            using (var context = new ApplicationDbContext(options, new BypassTenantContext(tenantId)))
            {
                var handoverService = new ShiftHandoverService(context);

                // Act
                var list = await handoverService.GetByPatientAsync(patientId, tenantId);
                var latest = await handoverService.GetLatestHandoversAsync(tenantId, 10);

                // Assert
                Assert.Single(list);
                Assert.Equal("nurse1", list[0].OutgoingNurseId);
                Assert.Equal("nurse2", list[0].IncomingNurseId);
                Assert.Equal("Stable patient. IV line running.", list[0].HandoverNotes);

                Assert.Single(latest);
            }
        }
    }
}
