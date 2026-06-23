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
using Microsoft.Extensions.Caching.Memory;

namespace CareSphere.Modules.Admin.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);
        private readonly System.Threading.SemaphoreSlim _semaphore = new(1, 1);

        public PermissionService(
            ApplicationDbContext context,
            IMemoryCache cache,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _cache = cache;
            _userManager = userManager;
        }

        public async Task<bool> UserHasPermissionAsync(Guid tenantId, string userId, string permission)
        {
            var cacheKey = $"permission_{tenantId}_{userId}_{permission}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                return cachedResult;

            await _semaphore.WaitAsync();
            try
            {
                // Re-check the cache after acquiring the lock to avoid redundant DB queries
                if (_cache.TryGetValue(cacheKey, out cachedResult))
                    return cachedResult;

                // 1. Check for an explicit non-revoked grant for this exact permission
                var explicitGrant = await _context.UserPermissions
                    .FirstOrDefaultAsync(up =>
                        up.TenantId == tenantId &&
                        up.UserId == userId &&
                        up.Permission == permission &&
                        !up.IsRevoked);

                if (explicitGrant != null)
                {
                    _cache.Set(cacheKey, true, CacheExpiry);
                    return true;
                }

                // 2. Check for an explicit revocation — if revoked, deny immediately
                var explicitRevoke = await _context.UserPermissions
                    .FirstOrDefaultAsync(up =>
                        up.TenantId == tenantId &&
                        up.UserId == userId &&
                        up.Permission == permission &&
                        up.IsRevoked);

                if (explicitRevoke != null)
                {
                    _cache.Set(cacheKey, false, CacheExpiry);
                    return false;
                }

                // 3. Fall back to role-based permissions
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.TenantId != tenantId)
                {
                    _cache.Set(cacheKey, false, CacheExpiry);
                    return false;
                }

                var rolePerms = await GetRolePermissionsAsync(tenantId, user.Role);
                var result = rolePerms.Contains(permission);
                _cache.Set(cacheKey, result, CacheExpiry);
                return result;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<string>> GetUserPermissionsAsync(Guid tenantId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            // Start with role defaults
            var rolePerms = await GetRolePermissionsAsync(tenantId, user.Role);
            var effectivePerms = new HashSet<string>(rolePerms);

            // Apply explicit grants and revocations
            var userPerms = await _context.UserPermissions.AsNoTracking()
                .Where(up => up.TenantId == tenantId && up.UserId == userId)
                .ToListAsync();

            foreach (var up in userPerms)
            {
                if (up.IsRevoked)
                    effectivePerms.Remove(up.Permission);
                else
                    effectivePerms.Add(up.Permission);
            }

            return effectivePerms.ToList();
        }

        public async Task<PermissionResult> GrantPermissionAsync(Guid tenantId, string userId, string permission, string grantedByUserId)
        {
            // Remove any existing entry for this user+permission before re-granting
            var existing = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.TenantId == tenantId && up.UserId == userId && up.Permission == permission);

            if (existing != null)
            {
                existing.IsRevoked = false;
                existing.RevokedAt = null;
                existing.GrantedAt = DateTime.UtcNow;
                existing.GrantedByUserId = grantedByUserId;
            }
            else
            {
                _context.UserPermissions.Add(new UserPermission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = userId,
                    Permission = permission,
                    GrantedAt = DateTime.UtcNow,
                    GrantedByUserId = grantedByUserId,
                    IsRevoked = false,
                });
            }

            await _context.SaveChangesAsync();
            InvalidateUserPermissionCache(tenantId, userId);
            return PermissionResult.Ok();
        }

        public async Task<PermissionResult> RevokePermissionAsync(Guid tenantId, string userId, string permission)
        {
            var existing = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.TenantId == tenantId && up.UserId == userId && up.Permission == permission);

            if (existing == null)
            {
                // Create an explicit revocation entry so role default is overridden
                _context.UserPermissions.Add(new UserPermission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = userId,
                    Permission = permission,
                    GrantedAt = DateTime.UtcNow,
                    GrantedByUserId = "system",
                    IsRevoked = true,
                    RevokedAt = DateTime.UtcNow,
                });
            }
            else
            {
                existing.IsRevoked = true;
                existing.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            InvalidateUserPermissionCache(tenantId, userId);
            return PermissionResult.Ok();
        }

        public async Task<List<string>> GetRolePermissionsAsync(Guid tenantId, string roleName)
        {
            // Load tenant-specific overrides from DB
            var dbPerms = await _context.RolePermissions.AsNoTracking()
                .Where(rp => rp.TenantId == tenantId && rp.RoleName == roleName)
                .Select(rp => rp.Permission)
                .ToListAsync();

            // If tenant has its own overrides, use those
            if (dbPerms.Count > 0)
                return dbPerms;

            // Otherwise fall back to the code defaults
            if (RolePermissionDefaults.DefaultPermissions.TryGetValue(roleName, out var defaults))
                return new List<string>(defaults);

            return new List<string>();
        }

        public async Task<PermissionResult> UpdateRolePermissionsAsync(Guid tenantId, string roleName, List<string> permissions, string updatedByUserId)
        {
            // Remove existing role permissions for this tenant+role
            var existing = await _context.RolePermissions
                .Where(rp => rp.TenantId == tenantId && rp.RoleName == roleName)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            // Insert new set
            foreach (var perm in permissions)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RoleName = roleName,
                    Permission = perm,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            await _context.SaveChangesAsync();

            // Invalidate cache for all users with this role in this tenant
            var usersInRole = await _context.Users
                .Where(u => ((ApplicationUser)u).TenantId == tenantId && ((ApplicationUser)u).Role == roleName)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var uid in usersInRole)
                InvalidateUserPermissionCache(tenantId, uid);

            return PermissionResult.Ok();
        }

        public async Task SeedRolePermissionsAsync(Guid tenantId)
        {
            // Only seed if no role permissions exist for this tenant
            var hasSeeded = await _context.RolePermissions.AnyAsync(rp => rp.TenantId == tenantId);
            if (hasSeeded) return;

            foreach (var (role, perms) in RolePermissionDefaults.DefaultPermissions)
            {
                foreach (var perm in perms)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        RoleName = role,
                        Permission = perm,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public void InvalidateUserPermissionCache(Guid tenantId, string userId)
        {
            // Invalidate all permission cache keys for this user by removing known patterns.
            // Since IMemoryCache doesn't support pattern removal, we track individual permission strings.
            var allPermissions = RolePermissionDefaults.DefaultPermissions.Values
                .SelectMany(x => x)
                .Distinct();
            
            foreach (var perm in allPermissions)
                _cache.Remove($"permission_{tenantId}_{userId}_{perm}");
        }
    }
}
