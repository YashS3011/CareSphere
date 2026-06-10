using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(ApplicationDbContext context, IAuditService auditService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string CurrentUserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

        public async Task<TenantSettings> GetTenantSettingsAsync(Guid tenantId)
        {
            try
            {
                CareSphere.Infrastructure.TenantContext.BypassTenantId = tenantId;
                var settings = await _context.TenantSettings.FirstOrDefaultAsync(t => t.TenantId == tenantId);
                if (settings == null)
                {
                    // Create defaults
                    settings = new TenantSettings
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        HospitalName = "CareSphere Hospital",
                        CreatedAt = DateTime.UtcNow,
                    };
                    _context.TenantSettings.Add(settings);
                    await _context.SaveChangesAsync();
                }
                return settings;
            }
            finally
            {
                CareSphere.Infrastructure.TenantContext.BypassTenantId = null;
            }
        }

        public async Task<TenantSettings> UpdateTenantSettingsAsync(TenantSettings settings, string updatedByUserId)
        {
            var existing = await _context.TenantSettings.FirstOrDefaultAsync(t => t.TenantId == settings.TenantId);
            if (existing == null)
                return await GetTenantSettingsAsync(settings.TenantId);

            existing.HospitalName = settings.HospitalName;
            existing.HospitalLogoPath = settings.HospitalLogoPath;
            existing.AddressLine1 = settings.AddressLine1;
            existing.AddressLine2 = settings.AddressLine2;
            existing.City = settings.City;
            existing.State = settings.State;
            existing.PinCode = settings.PinCode;
            existing.Phone = settings.Phone;
            existing.Email = settings.Email;
            existing.NabhRegistrationNumber = settings.NabhRegistrationNumber;
            existing.AbdmFacilityId = settings.AbdmFacilityId;
            
            existing.MaxUsersAllowed = settings.MaxUsersAllowed;
            existing.SubscriptionTier = settings.SubscriptionTier;
            existing.SsoEnabled = settings.SsoEnabled;
            existing.IsActive = settings.IsActive;

            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = settings.TenantId,
                UserId = updatedByUserId,
                Action = "TENANT_SETTINGS_UPDATED",
                ResourceType = "TenantSettings",
                ResourceId = existing.Id.ToString(),
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            });

            return existing;
        }

        public async Task<TenantSettings> ConfigureSsoAsync(Guid tenantId, bool ssoEnabled, string? provider, string? clientId, string? clientSecret, string? authority, string? samlMetadataUrl, string updatedByUserId)
        {
            var existing = await GetTenantSettingsAsync(tenantId);

            existing.SsoEnabled = ssoEnabled;
            existing.SsoProvider = provider;
            existing.OidcClientId = clientId;
            existing.OidcClientSecret = clientSecret;
            existing.OidcAuthority = authority;
            existing.SamlMetadataUrl = samlMetadataUrl;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = tenantId,
                UserId = updatedByUserId,
                Action = "SSO_CONFIGURED",
                ResourceType = "TenantSettings",
                ResourceId = existing.Id.ToString(),
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            });

            return existing;
        }

        public async Task<TenantStats> GetTenantStatsAsync(Guid tenantId)
        {
            var totalUsers = await _context.Users
                .Cast<ApplicationUser>()
                .CountAsync(u => u.TenantId == tenantId);

            var activeUsers = await _context.Users
                .Cast<ApplicationUser>()
                .CountAsync(u => u.TenantId == tenantId && u.IsActive);

            var totalPatients = await _context.Patients
                .CountAsync(p => p.TenantId == tenantId);

            var totalBeds = await _context.Beds
                .CountAsync(b => b.TenantId == tenantId);

            var availableBeds = await _context.Beds
                .CountAsync(b => b.TenantId == tenantId && b.Status == "Available");

            var totalDoctors = await _context.Doctors
                .CountAsync(d => d.TenantId == tenantId);

            var today = DateTime.UtcNow.Date;
            var activeEncountersToday = await _context.Encounters
                .CountAsync(e => e.TenantId == tenantId && e.CreatedAt >= today);

            var activeSessions = await _context.UserSessions
                .CountAsync(s => s.TenantId == tenantId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);

            var last24h = DateTime.UtcNow.AddHours(-24);
            var auditEvents24h = await _context.AuditEvents
                .CountAsync(a => a.TenantId == tenantId && a.Timestamp >= last24h);

            var userCountsByRole = await _context.Users
                .Cast<ApplicationUser>()
                .Where(u => u.TenantId == tenantId)
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Role, x => x.Count);

            var settings = await _context.TenantSettings
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);

            return new TenantStats
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalPatients = totalPatients,
                TotalBeds = totalBeds,
                AvailableBeds = availableBeds,
                SubscriptionTier = settings?.SubscriptionTier ?? "Free",
                TotalDoctors = totalDoctors,
                ActiveEncountersToday = activeEncountersToday,
                ActiveSessions = activeSessions,
                TotalAuditEvents24h = auditEvents24h,
                MaxUsersAllowed = settings?.MaxUsersAllowed ?? 100,
                UserCountsByRole = userCountsByRole,
            };
        }
    }
}
