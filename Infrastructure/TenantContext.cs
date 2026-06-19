using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CareSphere.Infrastructure
{
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly AsyncLocal<Guid?> _bypassTenantId = new();

        // Explicit override set by the Blazor auth layer when cookies are absent.
        private Guid? _overrideTenantId;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets or sets an ambient tenant ID that bypasses HttpContext checks.
        /// Useful for database seeding, migrations, and background jobs.
        /// </summary>
        public static Guid? BypassTenantId
        {
            get => _bypassTenantId.Value;
            set => _bypassTenantId.Value = value;
        }

        /// <summary>
        /// Explicitly sets the tenant ID for this scoped instance.
        /// Called by TabIsolatedAuthenticationStateProvider after resolving a valid session,
        /// so that EF query filters work correctly even without a cookie.
        /// </summary>
        public void SetTenantId(Guid tenantId)
        {
            _overrideTenantId = tenantId;
        }

        public Guid TenantId
        {
            get
            {
                // 1. Background job / seeder bypass
                if (_bypassTenantId.Value.HasValue)
                    return _bypassTenantId.Value.Value;

                // 2. Explicitly set by the Blazor auth layer (window.name session)
                if (_overrideTenantId.HasValue)
                    return _overrideTenantId.Value;

                // 3. Fallback: read from cookie-based HttpContext.User (legacy path)
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return Guid.Empty;

                var value = httpContext.User?.FindFirstValue("tenant_id")
                            ?? httpContext.User?.FindFirstValue(CareSphereClaimTypes.TenantId);

                if (string.IsNullOrEmpty(value))
                    return Guid.Empty;

                return Guid.Parse(value);
            }
        }
    }
}
