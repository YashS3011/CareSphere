using CareSphere.Data;
using CareSphere.Models;
using CareSphere.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CareSphere.Services
{
    public interface IImpersonationService
    {
        Task<ImpersonationResult> ImpersonateAsync(Guid targetTenantId, string targetUserId);
        Task EndImpersonationAsync();
    }

    public class ImpersonationResult
    {
        public string ShortLivedToken { get; set; } = string.Empty;
        public string TargetUserFullName { get; set; } = string.Empty;
        public string TargetHospitalName { get; set; } = string.Empty;
        public Guid AuditEventId { get; set; }
    }

    public class ImpersonationService : IImpersonationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImpersonationService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ImpersonationResult> ImpersonateAsync(Guid targetTenantId, string targetUserId)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser == null || !currentUser.Identity?.IsAuthenticated == true)
            {
                throw new InvalidOperationException("User is not authenticated.");
            }

            var superAdminUserId = CurrentUserHelper.GetUserId(currentUser);
            if (string.IsNullOrEmpty(superAdminUserId))
            {
                superAdminUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
            }

            // Load target user ignoring query filters
            var targetUser = await _context.Users
                .IgnoreQueryFilters()
                .Cast<ApplicationUser>()
                .FirstOrDefaultAsync(u => u.Id == targetUserId);

            if (targetUser == null)
            {
                throw new ArgumentException($"Target user with ID {targetUserId} not found.");
            }

            // Load target tenant settings ignoring query filters
            var targetTenant = await _context.TenantSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TenantId == targetTenantId);

            if (targetTenant == null)
            {
                throw new ArgumentException($"Target tenant with ID {targetTenantId} not found.");
            }

            // Load target user's permissions
            var permissions = await _permissionService.GetUserPermissionsAsync(targetTenantId, targetUserId);

            // Construct claims matching the normal login structure + impersonation details
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, targetUser.Id),
                new Claim(ClaimTypes.Name, targetUser.UserName ?? targetUser.Email ?? ""),
                new Claim(ClaimTypes.Email, targetUser.Email ?? ""),
                new Claim(ClaimTypes.Role, targetUser.Role),
                new Claim("role", targetUser.Role),
                new Claim(CareSphereClaimTypes.TenantId, targetTenantId.ToString()),
                new Claim(CareSphereClaimTypes.FullName, targetUser.FullName),
                new Claim("impersonated_by", superAdminUserId),
                new Claim("impersonation_session", "true"),
                new Claim("target_hospital_name", targetTenant.HospitalName)
            };

            if (targetUser.DoctorId.HasValue)
            {
                claims.Add(new Claim(CareSphereClaimTypes.DoctorId, targetUser.DoctorId.Value.ToString()));
            }

            foreach (var perm in permissions)
            {
                claims.Add(new Claim(CareSphereClaimTypes.Permission, perm));
            }

            // Generate short-lived JWT token (15 mins expiry)
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _configuration["Supabase:JwtSecret"];
            if (string.IsNullOrEmpty(secret))
            {
                // Local dev fallback
                secret = "local_dev_secret_key_for_caresphere_1234567890";
            }

            var key = Encoding.UTF8.GetBytes(secret);
            if (key.Length < 32)
            {
                Array.Resize(ref key, 32);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenObj = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(tokenObj);

            // Write append-only AuditEvent directly via db context
            var detailsObj = new { hospital = targetTenant.HospitalName, duration = "15min" };
            var detailsJson = JsonSerializer.Serialize(detailsObj);

            var auditEvent = new AuditEvent
            {
                Id = Guid.NewGuid(),
                UserId = superAdminUserId,
                Action = "SuperAdmin_Impersonation",
                ResourceType = "User",
                ResourceId = targetUserId,
                Timestamp = DateTime.UtcNow,
                TenantId = targetTenantId,
                Details = detailsJson,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                Device = "Desktop" // Default placeholder
            };

            _context.AuditEvents.Add(auditEvent);
            await _context.SaveChangesAsync();

            return new ImpersonationResult
            {
                ShortLivedToken = tokenString,
                TargetUserFullName = targetUser.FullName,
                TargetHospitalName = targetTenant.HospitalName,
                AuditEventId = auditEvent.Id
            };
        }

        public async Task EndImpersonationAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var currentPrincipal = httpContext.User;
            var superAdminUserId = currentPrincipal.FindFirstValue("impersonated_by");

            if (!string.IsNullOrEmpty(superAdminUserId))
            {
                // Clear the cookie
                httpContext.Response.Cookies.Delete("impersonation_token");

                // Write audit event
                var auditEvent = new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    UserId = superAdminUserId,
                    Action = "SuperAdmin_Impersonation_Ended",
                    ResourceType = "User",
                    ResourceId = superAdminUserId,
                    Timestamp = DateTime.UtcNow,
                    TenantId = Guid.Empty, // Platform level
                    IpAddress = httpContext.Connection?.RemoteIpAddress?.ToString(),
                    Device = "Desktop"
                };

                _context.AuditEvents.Add(auditEvent);
                await _context.SaveChangesAsync();
            }
        }
    }
}
