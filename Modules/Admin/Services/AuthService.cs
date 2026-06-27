using CareSphere.Modules.Clinical.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Patients.Services;
using CareSphere.Modules.Ward.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Shared.Services;
using CareSphere.Modules.Shared.Events;
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareSphere.Modules.Admin.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IPermissionService _permissionService;

        public AuthService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IAuditService auditService,
            IPermissionService permissionService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _auditService = auditService;
            _permissionService = permissionService;
        }

        public async Task<AuthResult> LoginAsync(string email, string password, Guid tenantId, string? ipAddress, string? userAgent)
        {
            try
            {
                var user = await _userManager.Users.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());
                if (user == null)
                    return AuthResult.Fail("Invalid email or password.");

                bool isPlatformAdmin = user.Role == "platform_super_admin" || user.TenantId == Guid.Empty;
                var effectiveTenantId = isPlatformAdmin ? user.TenantId : tenantId;

                if (!isPlatformAdmin && user.TenantId != tenantId)
                    return AuthResult.Fail("Invalid email or password.");

                CareSphere.Infrastructure.TenantContext.BypassTenantId = effectiveTenantId;

                if (!user.IsActive)
                    return AuthResult.Fail("Account is deactivated. Please contact your administrator.");

                if (!isPlatformAdmin)
                {
                    var tenantSettings = await _context.TenantSettings
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(t => t.TenantId == user.TenantId);
                    if (tenantSettings == null || !tenantSettings.IsActive)
                        return AuthResult.Fail("This hospital is currently suspended. Please contact platform operations.");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                        return AuthResult.Fail("Account is locked out due to too many failed attempts. Try again in 15 minutes.");
                    return AuthResult.Fail("Invalid email or password.");
                }

                // Build permission claims
                var permissions = await _permissionService.GetUserPermissionsAsync(effectiveTenantId, user.Id);
                var claims = new List<Claim>
                {
                    new Claim(CareSphereClaimTypes.TenantId, user.TenantId.ToString()),
                    new Claim(CareSphereClaimTypes.FullName, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("role", user.Role),
                };

                if (user.DoctorId.HasValue)
                    claims.Add(new Claim(CareSphereClaimTypes.DoctorId, user.DoctorId.Value.ToString()));

                if (user.PatientId.HasValue)
                    claims.Add(new Claim(CareSphereClaimTypes.PatientId, user.PatientId.Value.ToString()));

                foreach (var perm in permissions)
                    claims.Add(new Claim(CareSphereClaimTypes.Permission, perm));

                // Create a session record
                var sessionToken = Guid.NewGuid().ToString("N");
                _context.UserSessions.Add(new UserSession
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = user.Id,
                    SessionToken = sessionToken,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceInfo = ParseDeviceInfo(userAgent),
                    CreatedAt = DateTime.UtcNow,
                    LastActivityAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(8),
                    IsRevoked = false,
                });

                // Update last login timestamp
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();

                // Sign in with extended claims (cookie auth)
                try
                {
                    if (_signInManager.Context?.Response?.HasStarted == false)
                    {
                        await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, claims);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Ignore: response has already started in Blazor Interactive Server mode.
                    // Tab session isolation is managed via JS window.name token.
                }

                // Audit
                await _auditService.LogAsync(new AuditEvent
                {
                    TenantId = tenantId,
                    UserId = user.Id,
                    Action = "USER_LOGIN",
                    ResourceType = "User",
                    ResourceId = user.Id,
                    IpAddress = ipAddress,
                    Device = ParseDeviceInfo(userAgent),
                });

                return AuthResult.Ok(user, sessionToken);
            }
            finally
            {
                CareSphere.Infrastructure.TenantContext.BypassTenantId = null;
            }
        }

        public async Task LogoutAsync(string userId, string sessionToken)
        {
            // Revoke the specific session
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SessionToken == sessionToken && !s.IsRevoked);

            if (session != null)
            {
                session.IsRevoked = true;
                session.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var user = await _userManager.FindByIdAsync(userId);
            var tenantId = user?.TenantId ?? Guid.Empty;

            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = tenantId,
                UserId = userId,
                Action = "USER_LOGOUT",
                ResourceType = "User",
                ResourceId = userId,
            });

            try
            {
                if (_signInManager.Context?.Response?.HasStarted == false)
                {
                    await _signInManager.SignOutAsync();
                }
            }
            catch (InvalidOperationException)
            {
                // Ignore: response has already started in Blazor Interactive Server mode.
            }
        }

        public async Task<bool> RefreshSessionAsync(string sessionToken)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && !s.IsRevoked);

            if (session == null || session.ExpiresAt < DateTime.UtcNow)
                return false;

            session.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateSessionAsync(string sessionToken)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && !s.IsRevoked);

            return session != null && session.ExpiresAt > DateTime.UtcNow;
        }

        public async Task RevokeAllSessionsAsync(string userId)
        {
            var sessions = await _context.UserSessions
                .Where(s => s.UserId == userId && !s.IsRevoked)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsRevoked = true;
                session.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private static string ParseDeviceInfo(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";
            if (userAgent.Contains("Mobile")) return "Mobile";
            if (userAgent.Contains("Tablet")) return "Tablet";
            return "Desktop";
        }
    }
}
