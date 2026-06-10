using Microsoft.AspNetCore.Authorization;
using CareSphere.Services;
using System.Security.Claims;

namespace CareSphere.Authorization
{
    /// <summary>
    /// Handles permission-based authorization by checking the user's permissions
    /// against the required permission in PermissionRequirement.
    /// Checks explicit user grants first, then falls back to role defaults.
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                context.Fail();
                return;
            }

            var tenantIdClaim = context.User.FindFirstValue(CareSphereClaimTypes.TenantId);
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(tenantIdClaim) || string.IsNullOrEmpty(userId))
            {
                context.Fail();
                return;
            }

            if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                context.Fail();
                return;
            }

            var hasPermission = await _permissionService.UserHasPermissionAsync(tenantId, userId, requirement.Permission);

            if (hasPermission)
                context.Succeed(requirement);
            else
                context.Fail();
        }
    }
}
