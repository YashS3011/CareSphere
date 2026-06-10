using CareSphere.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareSphere.Infrastructure
{
    /// <summary>
    /// Runs once at application startup to seed the database with required initial data.
    /// Creates default Identity roles, the SuperAdmin user, default TenantSettings,
    /// and seeds role permissions.
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CareSphere.Services.ITenantService _tenantService;
        private readonly CareSphere.Services.IPermissionService _permissionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            CareSphere.Services.ITenantService tenantService,
            CareSphere.Services.IPermissionService permissionService,
            IConfiguration configuration,
            ILogger<DatabaseSeeder> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _tenantService = tenantService;
            _permissionService = permissionService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            var defaultTenantIdStr = _configuration["App:DefaultTenantId"] ?? "00000000-0000-0000-0000-000000000001";
            var defaultTenantId = Guid.Parse(defaultTenantIdStr);

            // Use BypassTenantContext to handle seed context environment
            var bypassContext = new BypassTenantContext(defaultTenantId);
            TenantContext.BypassTenantId = bypassContext.TenantId;

            // 1. Create all system roles
            var allRoles = new[]
            {
                CareSphereRoles.SuperAdmin, CareSphereRoles.HospitalAdmin,
                CareSphereRoles.Doctor, CareSphereRoles.Nurse,
                CareSphereRoles.Pharmacist, CareSphereRoles.LabTechnician,
                CareSphereRoles.FrontDesk, CareSphereRoles.Finance,
                CareSphereRoles.NabhAuditor, CareSphereRoles.Patient,
            };

            foreach (var role in allRoles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                    _logger.LogInformation("Created role: {Role}", role);
                }
            }

            // 2. Create default SuperAdmin user if no users exist at all
            if (!_userManager.Users.Any())
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@caresphere.in",
                    Email = "admin@caresphere.in",
                    FullName = "System Administrator",
                    TenantId = defaultTenantId,
                    Role = CareSphereRoles.SuperAdmin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PreferredLanguage = "en",
                    EmailConfirmed = true,
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, CareSphereRoles.SuperAdmin);
                    _logger.LogInformation("Created default SuperAdmin user: admin@caresphere.in");
                }
                else
                {
                    _logger.LogError("Failed to create SuperAdmin: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // Seed a test platform admin user during seeding for local development
            var platformAdminEmail = "platformadmin@caresphere.dev";
            var platformAdmin = await _userManager.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.NormalizedEmail == platformAdminEmail.ToUpperInvariant());
            if (platformAdmin == null)
            {
                // LOCAL DEV ONLY — remove or restrict in production
                var platformAdminUser = new ApplicationUser
                {
                    UserName = platformAdminEmail,
                    Email = platformAdminEmail,
                    FullName = "Platform Super Admin",
                    TenantId = Guid.Empty, // Dedicated platform tenant Guid or Empty
                    Role = "platform_super_admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PreferredLanguage = "en",
                    EmailConfirmed = true,
                };

                var result = await _userManager.CreateAsync(platformAdminUser, "PlatformAdmin@123");
                if (result.Succeeded)
                {
                    await _userManager.AddClaimAsync(platformAdminUser, new Claim("role", "platform_super_admin"));
                    _logger.LogInformation("Created test platform admin user: platformadmin@caresphere.dev");
                }
                else
                {
                    _logger.LogError("Failed to create Platform Super Admin: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // 3. Create default TenantSettings if none exist
            await _tenantService.GetTenantSettingsAsync(defaultTenantId); // Creates defaults if not found

            // 4. Seed role permissions for the default tenant
            await _permissionService.SeedRolePermissionsAsync(defaultTenantId);

            _logger.LogInformation("Database seeding completed for tenant: {TenantId}", defaultTenantId);

            // Clear bypass context after seeding is completed
            TenantContext.BypassTenantId = null;
        }
    }
}
