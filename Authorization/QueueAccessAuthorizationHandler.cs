using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using CareSphere.Modules.Admin.Services;
using CareSphere.Infrastructure;

namespace CareSphere.Authorization
{
    /// <summary>
    /// Evaluates if the current user meets the QueueAccessRequirement by checking
    /// for Queue_View, Queue_Manage, or Encounters_View permissions using the PermissionService.
    /// </summary>
    public class QueueAccessAuthorizationHandler : AuthorizationHandler<QueueAccessRequirement>
    {
        private readonly IPermissionService _permissionService;

        public QueueAccessAuthorizationHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            QueueAccessRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
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

            // A user has queue access if they have Queue_View OR Queue_Manage OR Encounters_View permissions
            var hasView = await _permissionService.UserHasPermissionAsync(tenantId, userId, CareSpherePermissions.Queue_View);
            var hasManage = await _permissionService.UserHasPermissionAsync(tenantId, userId, CareSpherePermissions.Queue_Manage);
            var hasEncounters = await _permissionService.UserHasPermissionAsync(tenantId, userId, CareSpherePermissions.Encounters_View);

            if (hasView || hasManage || hasEncounters)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
