using System.Security.Claims;

namespace CareSphere.Infrastructure
{
    /// <summary>
    /// Static helper methods for reading CareSphere-specific claims from a ClaimsPrincipal.
    /// Use in Blazor pages to conditionally show/hide UI elements based on the current user.
    /// </summary>
    public static class CurrentUserHelper
    {
        /// <summary>Returns the current user's Identity ID (from ClaimTypes.NameIdentifier).</summary>
        public static string? GetUserId(ClaimsPrincipal principal)
            => principal.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>Returns the current user's TenantId parsed as a Guid.</summary>
        public static Guid? GetTenantId(ClaimsPrincipal principal)
        {
            var val = principal.FindFirstValue(CareSphereClaimTypes.TenantId);
            return Guid.TryParse(val, out var id) ? id : null;
        }

        /// <summary>Returns the current user's role.</summary>
        public static string? GetUserRole(ClaimsPrincipal principal)
            => principal.FindFirstValue(ClaimTypes.Role);

        /// <summary>Returns the current user's full display name.</summary>
        public static string? GetFullName(ClaimsPrincipal principal)
            => principal.FindFirstValue(CareSphereClaimTypes.FullName);

        /// <summary>Returns the linked DoctorId if the user is a Doctor, otherwise null.</summary>
        public static Guid? GetDoctorId(ClaimsPrincipal principal)
        {
            var val = principal.FindFirstValue(CareSphereClaimTypes.DoctorId);
            return Guid.TryParse(val, out var id) ? id : null;
        }

        /// <summary>
        /// Checks whether the given permission is present as a claim on the principal.
        /// Fast in-process check — does not query the database.
        /// </summary>
        public static bool HasPermission(ClaimsPrincipal principal, string permission)
            => principal.HasClaim(CareSphereClaimTypes.Permission, permission);

        /// <summary>Returns true if the user is authenticated.</summary>
        public static bool IsAuthenticated(ClaimsPrincipal principal)
            => principal.Identity?.IsAuthenticated ?? false;
    }
}
