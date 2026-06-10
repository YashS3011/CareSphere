// Regression guard: must pass on every PR to ensure tenant isolation is never broken.
using System;
using System.Threading.Tasks;
using CareSphere.Data;
using CareSphere.Infrastructure;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace CareSphere.Tests
{
    public class MultiTenantIsolationTests : IAsyncLifetime
    {
        private PostgreSqlContainer? _postgresContainer;
        private bool _dockerAvailable = false;

        public async Task InitializeAsync()
        {
            try
            {
                _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
                    .Build();
                await _postgresContainer.StartAsync();
                _dockerAvailable = true;
            }
            catch
            {
                _dockerAvailable = false;
                _postgresContainer = null;
            }
        }

        public async Task DisposeAsync()
        {
            if (_dockerAvailable && _postgresContainer != null)
            {
                await _postgresContainer.DisposeAsync();
            }
        }

        [Fact]
        public async Task VerifyTenantIsolationQueryFilter()
        {
            DbContextOptions<ApplicationDbContext> options;

            if (_dockerAvailable && _postgresContainer != null)
            {
                options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseNpgsql(_postgresContainer.GetConnectionString())
                    .Options;

                // Run migrations to initialize the schema
                using (var migrateContext = new ApplicationDbContext(options, new BypassTenantContext(Guid.Empty)))
                {
                    await migrateContext.Database.MigrateAsync();
                }
            }
            else
            {
                // Fallback to InMemory database if Docker is unavailable in this environment
                options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase("CareSphere_TenantIsolation_Test_" + Guid.NewGuid())
                    .Options;
            }

            // Declare two separate tenant IDs
            Guid tenantA = Guid.NewGuid();
            Guid tenantB = Guid.NewGuid();

            // Using BypassTenantContext(tenantA), seed one Patient row with TenantId = tenantA
            using (var contextA = new ApplicationDbContext(options, new BypassTenantContext(tenantA)))
            {
                var patientA = new Patient
                {
                    Id = Guid.NewGuid(),
                    Mrn = "MRN-A001",
                    FirstName = "Alice",
                    LastName = "Smith",
                    DateOfBirth = new DateTime(1990, 5, 12),
                    Gender = "Female",
                    Phone = "9876543210",
                    Email = "alice@tenantA.com",
                    TenantId = tenantA,
                    CreatedAt = DateTime.UtcNow
                };
                contextA.Patients.Add(patientA);
                await contextA.SaveChangesAsync();
            }

            // Using BypassTenantContext(tenantB), seed one Patient row with TenantId = tenantB
            using (var contextB = new ApplicationDbContext(options, new BypassTenantContext(tenantB)))
            {
                var patientB = new Patient
                {
                    Id = Guid.NewGuid(),
                    Mrn = "MRN-B001",
                    FirstName = "Bob",
                    LastName = "Jones",
                    DateOfBirth = new DateTime(1985, 10, 24),
                    Gender = "Male",
                    Phone = "1234567890",
                    Email = "bob@tenantB.com",
                    TenantId = tenantB,
                    CreatedAt = DateTime.UtcNow
                };
                contextB.Patients.Add(patientB);
                await contextB.SaveChangesAsync();
            }

            // Instantiate a fresh ApplicationDbContext using BypassTenantContext(tenantA)
            using (var context = new ApplicationDbContext(options, new BypassTenantContext(tenantA)))
            {
                // Call context.Patients.ToListAsync()
                var results = await context.Patients.ToListAsync();

                // Assert: results contains the tenantA patient
                Assert.Contains(results, p => p.TenantId == tenantA && p.Mrn == "MRN-A001");

                // Assert: results does NOT contain the tenantB patient
                Assert.DoesNotContain(results, p => p.TenantId == tenantB);
            }
        }
    }
}
