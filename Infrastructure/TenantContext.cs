using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CareSphere.Infrastructure
{
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly AsyncLocal<Guid?> _bypassTenantId = new();

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

        public Guid TenantId
        {
            get
            {
                if (_bypassTenantId.Value.HasValue)
                {
                    return _bypassTenantId.Value.Value;
                }

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    return Guid.Empty;
                }

                var value = httpContext.User?.FindFirstValue("tenant_id")
                            ?? httpContext.User?.FindFirstValue(CareSphereClaimTypes.TenantId);

                if (string.IsNullOrEmpty(value))
                {
                    return Guid.Empty;
                }

                return Guid.Parse(value);
            }
        }
    }
}
