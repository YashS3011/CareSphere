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

namespace CareSphere.Modules.Admin.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IAuditService auditService,
            IAuthService authService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _auditService = auditService;
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string CurrentUserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

        public async Task<UserCreateResult> CreateUserAsync(Guid tenantId, string fullName, string email, string password, string role, string? department = null, Guid? doctorId = null)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                TenantId = tenantId,
                Role = role,
                Department = department,
                DoctorId = doctorId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PreferredLanguage = "en",
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return UserCreateResult.Fail(result.Errors.Select(e => e.Description));

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);

            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = tenantId,
                UserId = CurrentUserId,
                Action = "USER_CREATED",
                ResourceType = "User",
                ResourceId = user.Id,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            });

            return UserCreateResult.Ok(user);
        }

        public async Task<UserCreateResult> UpdateUserAsync(string userId, string fullName, string? department, bool isActive, string preferredLanguage, Guid? doctorId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return UserCreateResult.Fail(new[] { "User not found." });

            user.FullName = fullName;
            user.Department = department;
            user.IsActive = isActive;
            user.PreferredLanguage = preferredLanguage;
            user.DoctorId = doctorId;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return UserCreateResult.Fail(result.Errors.Select(e => e.Description));

            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = user.TenantId,
                UserId = CurrentUserId,
                Action = "USER_UPDATED",
                ResourceType = "User",
                ResourceId = user.Id,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            });

            return UserCreateResult.Ok(user);
        }

        public async Task<UserCreateResult> ResetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return UserCreateResult.Fail(new[] { "User not found." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                return UserCreateResult.Fail(result.Errors.Select(e => e.Description));

            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = user.TenantId,
                UserId = CurrentUserId,
                Action = "PASSWORD_RESET",
                ResourceType = "User",
                ResourceId = user.Id,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            });

            return UserCreateResult.Ok(user);
        }

        public async Task<UserCreateResult> ToggleUserActiveAsync(string userId, string performedByUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return UserCreateResult.Fail(new[] { "User not found." });

            user.IsActive = !user.IsActive;
            var action = user.IsActive ? "USER_ACTIVATED" : "USER_DEACTIVATED";

            if (!user.IsActive)
            {
                // Revoke all active sessions when deactivating
                await _authService.RevokeAllSessionsAsync(userId);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return UserCreateResult.Fail(result.Errors.Select(e => e.Description));

            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = user.TenantId,
                UserId = performedByUserId,
                Action = action,
                ResourceType = "User",
                ResourceId = user.Id,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            });

            return UserCreateResult.Ok(user);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<List<UserListItem>> GetUsersByTenantAsync(Guid tenantId, string? search = null, string? role = null, bool? isActive = null)
        {
            var query = _context.Users
                .Cast<ApplicationUser>()
                .Where(u => u.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(lower) ||
                    (u.Email != null && u.Email.ToLower().Contains(lower)));
            }

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role == role);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            return await query
                .OrderBy(u => u.FullName)
                .Select(u => new UserListItem
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    Role = u.Role,
                    Department = u.Department,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt,
                    DoctorId = u.DoctorId,
                })
                .ToListAsync();
        }
    }
}
