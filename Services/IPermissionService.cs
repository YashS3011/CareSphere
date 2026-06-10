using CareSphere.Models;

namespace CareSphere.Services
{
    /// <summary>Result returned from IPermissionService methods.</summary>
    public class PermissionResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public static PermissionResult Ok() => new() { Success = true };
        public static PermissionResult Fail(string error) => new() { Success = false, Error = error };
    }

    public interface IPermissionService
    {
        /// <summary>
        /// Returns true if the user has the given permission in the specified tenant.
        /// Checks explicit grants first, then role defaults (with RolePermissions overrides).
        /// Results are cached for 5 minutes.
        /// </summary>
        Task<bool> UserHasPermissionAsync(Guid tenantId, string userId, string permission);

        /// <summary>Returns all effective permissions for a user (role defaults + explicit grants - revocations).</summary>
        Task<List<string>> GetUserPermissionsAsync(Guid tenantId, string userId);

        /// <summary>Grants a permission explicitly to a user. Invalidates cache.</summary>
        Task<PermissionResult> GrantPermissionAsync(Guid tenantId, string userId, string permission, string grantedByUserId);

        /// <summary>Revokes an explicit permission grant. Invalidates cache.</summary>
        Task<PermissionResult> RevokePermissionAsync(Guid tenantId, string userId, string permission);

        /// <summary>Returns the effective permission list for a role in a tenant (defaults + DB overrides).</summary>
        Task<List<string>> GetRolePermissionsAsync(Guid tenantId, string roleName);

        /// <summary>Upserts the role-specific permissions for a tenant. Invalidates affected user caches.</summary>
        Task<PermissionResult> UpdateRolePermissionsAsync(Guid tenantId, string roleName, List<string> permissions, string updatedByUserId);

        /// <summary>Seeds default role permissions from RolePermissionDefaults if not already seeded.</summary>
        Task SeedRolePermissionsAsync(Guid tenantId);

        /// <summary>Invalidates permission cache for all permissions of a specific user.</summary>
        void InvalidateUserPermissionCache(Guid tenantId, string userId);
    }
}
