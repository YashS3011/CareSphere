using CareSphere.Data;
using CareSphere.Infrastructure;
using CareSphere.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Modules.PlatformAdmin.Services
{
    /// <summary>
    /// Platform-Admin (Vendor) service that manages hospital tenants as clients.
    /// All operations bypass the per-tenant EF query filters.
    /// </summary>
    public class PlatformAdminService : IPlatformAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DatabaseSeeder _seeder;

        public PlatformAdminService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            DatabaseSeeder seeder)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _seeder = seeder;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SetBypass(Guid? id) => TenantContext.BypassTenantId = id;

        // ── Public API ────────────────────────────────────────────────────────────

        public async Task<List<TenantSettings>> GetAllHospitalsAsync()
        {
            SetBypass(Guid.Empty); // bypass tenant filter — IgnoreQueryFilters preferred
            try
            {
                return await _context.TenantSettings
                    .IgnoreQueryFilters()
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            finally { SetBypass(null); }
        }

        public async Task<(TenantSettings Tenant, string AdminEmail, string AdminPassword)> CreateHospitalAsync(
            string hospitalName, string? city, string? state, string? phone, string? email,
            string subscriptionTier, int maxUsers, string? addressLine1)
        {
            var newTenantId = Guid.NewGuid();

            // Create tenant settings
            var tenant = new TenantSettings
            {
                Id = Guid.NewGuid(),
                TenantId = newTenantId,
                HospitalName = hospitalName,
                City = city,
                State = state,
                Phone = phone,
                Email = email,
                AddressLine1 = addressLine1,
                SubscriptionTier = subscriptionTier,
                MaxUsersAllowed = maxUsers,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            SetBypass(newTenantId);
            try
            {
                _context.TenantSettings.Add(tenant);
                await _context.SaveChangesAsync();
            }
            finally { SetBypass(null); }

            // Auto-generate default HospitalAdmin credentials
            var safeHospitalSlug = new string(hospitalName
                .ToLower()
                .Where(c => char.IsLetterOrDigit(c))
                .Take(12)
                .ToArray());

            // Seed default settings and configurations (ERM with default values)
            await _seeder.SeedNewHospitalDefaultsAsync(newTenantId, hospitalName, safeHospitalSlug);

            var adminEmail    = $"admin@{safeHospitalSlug}.caresphere.local";
            var adminPassword = $"Admin@{DateTime.UtcNow:MMddyyyy}!";

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(CareSphereRoles.HospitalAdmin))
                await _roleManager.CreateAsync(new IdentityRole(CareSphereRoles.HospitalAdmin));

            var adminUser = new ApplicationUser
            {
                UserName          = adminEmail,
                Email             = adminEmail,
                EmailConfirmed    = true,
                FullName          = $"{hospitalName} Admin",
                TenantId          = newTenantId,
                Role              = CareSphereRoles.HospitalAdmin,
                IsActive          = true,
                PreferredLanguage = "en",
                CreatedAt         = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(adminUser, CareSphereRoles.HospitalAdmin);

            return (tenant, adminEmail, adminPassword);
        }

        public async Task FreezeHospitalUsersAsync(Guid tenantId)
        {
            var users = await _context.Users
                .Cast<ApplicationUser>()
                .IgnoreQueryFilters()
                .Where(u => u.TenantId == tenantId)
                .ToListAsync();

            foreach (var u in users) u.IsActive = false;

            // Also mark TenantSettings.IsActive = false (triggers global middleware lockout)
            var settings = await _context.TenantSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);
            if (settings != null) settings.IsActive = false;

            await _context.SaveChangesAsync();
        }

        public async Task UnfreezeHospitalUsersAsync(Guid tenantId)
        {
            var users = await _context.Users
                .Cast<ApplicationUser>()
                .IgnoreQueryFilters()
                .Where(u => u.TenantId == tenantId)
                .ToListAsync();

            foreach (var u in users) u.IsActive = true;

            var settings = await _context.TenantSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);
            if (settings != null) settings.IsActive = true;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteHospitalAsync(Guid tenantId)
        {
            // Remove tenant-scoped records — bulk delete via IgnoreQueryFilters
            // Note: Identity users must be removed via UserManager for cascade safety.
            var users = await _context.Users
                .Cast<ApplicationUser>()
                .IgnoreQueryFilters()
                .Where(u => u.TenantId == tenantId)
                .ToListAsync();

            foreach (var u in users)
                await _userManager.DeleteAsync(u);

            // Delete TenantSettings
            var settings = await _context.TenantSettings
                .IgnoreQueryFilters()
                .Where(t => t.TenantId == tenantId)
                .ToListAsync();
            _context.TenantSettings.RemoveRange(settings);

            // Remove tenant permissions
            var rolePerms = await _context.RolePermissions
                .IgnoreQueryFilters()
                .Where(r => r.TenantId == tenantId)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(rolePerms);

            await _context.SaveChangesAsync();
        }

        public async Task<HospitalClientStats> GetHospitalStatsAsync(Guid tenantId)
        {
            var totalUsers = await _context.Users
                .Cast<ApplicationUser>()
                .IgnoreQueryFilters()
                .CountAsync(u => u.TenantId == tenantId);

            var activeUsers = await _context.Users
                .Cast<ApplicationUser>()
                .IgnoreQueryFilters()
                .CountAsync(u => u.TenantId == tenantId && u.IsActive);

            var totalPatients = await _context.Patients
                .IgnoreQueryFilters()
                .CountAsync(p => p.TenantId == tenantId);

            var totalBeds = await _context.Beds
                .IgnoreQueryFilters()
                .CountAsync(b => b.TenantId == tenantId);

            var settings = await _context.TenantSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);

            return new HospitalClientStats
            {
                TotalUsers      = totalUsers,
                ActiveUsers     = activeUsers,
                TotalPatients   = totalPatients,
                TotalBeds       = totalBeds,
                SubscriptionTier = settings?.SubscriptionTier ?? "Free",
            };
        }

        public async Task UpdateHospitalSettingsAsync(TenantSettings updatedSettings)
        {
            SetBypass(updatedSettings.TenantId);
            try
            {
                var existing = await _context.TenantSettings
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(t => t.TenantId == updatedSettings.TenantId);
                if (existing == null)
                    throw new Exception("Hospital settings not found.");

                existing.HospitalName = updatedSettings.HospitalName;
                existing.Email = updatedSettings.Email;
                existing.AddressLine1 = updatedSettings.AddressLine1;
                existing.AddressLine2 = updatedSettings.AddressLine2;
                existing.City = updatedSettings.City;
                existing.State = updatedSettings.State;
                existing.PinCode = updatedSettings.PinCode;
                existing.Phone = updatedSettings.Phone;
                existing.NabhRegistrationNumber = updatedSettings.NabhRegistrationNumber;
                existing.AbdmFacilityId = updatedSettings.AbdmFacilityId;
                existing.SubscriptionTier = updatedSettings.SubscriptionTier;
                existing.MaxUsersAllowed = updatedSettings.MaxUsersAllowed;
                existing.SsoEnabled = updatedSettings.SsoEnabled;
                existing.SsoProvider = updatedSettings.SsoProvider;
                existing.OidcClientId = updatedSettings.OidcClientId;
                existing.OidcClientSecret = updatedSettings.OidcClientSecret;
                existing.OidcAuthority = updatedSettings.OidcAuthority;
                existing.SamlMetadataUrl = updatedSettings.SamlMetadataUrl;

                await _context.SaveChangesAsync();
            }
            finally { SetBypass(null); }
        }
    }
}
