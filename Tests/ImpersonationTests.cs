#nullable disable
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using CareSphere.Data;
using CareSphere.Infrastructure;
using CareSphere.Models;
using CareSphere.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CareSphere.Tests
{
    public class ImpersonationTests
    {
        [Fact]
        public async Task VerifySuperAdminImpersonationFlow()
        {
            // 1. Arrange: Setup DbContext options using InMemory DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("CareSphere_Impersonation_Test_" + Guid.NewGuid())
                .Options;

            var tenantId = Guid.NewGuid();
            var superAdminId = Guid.NewGuid().ToString();
            var targetUserId = Guid.NewGuid().ToString();

            // Seed target tenant settings
            using (var context = new ApplicationDbContext(options, new BypassTenantContext(tenantId)))
            {
                var settings = new TenantSettings
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    HospitalName = "Test Hospital",
                    MaxUsersAllowed = 50,
                    SubscriptionTier = "Professional",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.TenantSettings.Add(settings);

                var targetUser = new ApplicationUser
                {
                    Id = targetUserId,
                    UserName = "doctor@testhospital.com",
                    Email = "doctor@testhospital.com",
                    FullName = "Dr. Target Doctor",
                    TenantId = tenantId,
                    Role = "Doctor",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(targetUser);

                await context.SaveChangesAsync();
            }

            // Setup HttpContextAccessor with logged-in SuperAdmin user
            var superAdminClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, superAdminId),
                new Claim(CareSphereClaimTypes.FullName, "System Platform Admin"),
                new Claim(CareSphereClaimTypes.TenantId, Guid.Empty.ToString()),
                new Claim("role", "platform_super_admin")
            };
            var superAdminIdentity = new ClaimsIdentity(superAdminClaims, "TestAuth");
            var superAdminPrincipal = new ClaimsPrincipal(superAdminIdentity);

            var httpContext = new DefaultHttpContext { User = superAdminPrincipal };
            var mockAccessor = new HttpContextAccessor { HttpContext = httpContext };

            // Setup empty config (fallback secret will be used)
            var configData = new Dictionary<string, string>
            {
                { "Supabase:JwtSecret", "a_test_jwt_secret_key_for_caresphere_32_bytes_long" }
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

            // Setup service implementations
            using (var testContext = new ApplicationDbContext(options, new BypassTenantContext(tenantId)))
            {
                var userManager = new Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>(
                    new Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<ApplicationUser>(testContext),
                    null, null, null, null, null, null, null, null);

                var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
                var permissionService = new PermissionService(testContext, cache, userManager);
                var impersonationService = new ImpersonationService(testContext, configuration, permissionService, mockAccessor);

                // 2. Act: Perform Impersonation
                var result = await impersonationService.ImpersonateAsync(tenantId, targetUserId);

                // 3. Assert: Result verification
                Assert.NotNull(result);
                Assert.NotEmpty(result.ShortLivedToken);
                Assert.Equal("Dr. Target Doctor", result.TargetUserFullName);
                Assert.Equal("Test Hospital", result.TargetHospitalName);
                Assert.NotEqual(Guid.Empty, result.AuditEventId);

                // Decode token and verify claims
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(result.ShortLivedToken);
                
                Assert.Contains(jwtToken.Claims, c => c.Value == targetUserId);
                Assert.Contains(jwtToken.Claims, c => c.Value == "Doctor" && (c.Type == "role" || c.Type == ClaimTypes.Role || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"));
                Assert.Contains(jwtToken.Claims, c => c.Value == tenantId.ToString() && c.Type == "caresphere/tenant_id");
                Assert.Contains(jwtToken.Claims, c => c.Value == "true" && c.Type == "impersonation_session");
                Assert.Contains(jwtToken.Claims, c => c.Value == superAdminId && c.Type == "impersonated_by");

                // Verify Audit Event logged in DB
                var auditEvent = await testContext.AuditEvents.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == result.AuditEventId);
                Assert.NotNull(auditEvent);
                Assert.Equal("SuperAdmin_Impersonation", auditEvent.Action);
                Assert.Equal(superAdminId, auditEvent.UserId);
                Assert.Equal(targetUserId, auditEvent.ResourceId);
                Assert.Equal(tenantId, auditEvent.TenantId);
                Assert.NotNull(auditEvent.Details);

                var doc = JsonDocument.Parse(auditEvent.Details);
                Assert.Equal("Test Hospital", doc.RootElement.GetProperty("hospital").GetString());
                Assert.Equal("15min", doc.RootElement.GetProperty("duration").GetString());

                // 4. Act: End Impersonation
                // Setup current context as the impersonated user (carrying impersonated_by claim)
                var impersonatedClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, targetUserId),
                    new Claim("impersonated_by", superAdminId),
                    new Claim("impersonation_session", "true")
                };
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(impersonatedClaims, "TestAuth"));

                await impersonationService.EndImpersonationAsync();

                // 5. Assert: End Impersonation audit event exists
                var endAuditEvent = await testContext.AuditEvents.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(a => a.Action == "SuperAdmin_Impersonation_Ended" && a.UserId == superAdminId);
                Assert.NotNull(endAuditEvent);
                Assert.Equal("User", endAuditEvent.ResourceType);
                Assert.Equal(superAdminId, endAuditEvent.ResourceId);
                Assert.Equal(Guid.Empty, endAuditEvent.TenantId);
            }
        }
    }
}
