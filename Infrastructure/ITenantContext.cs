using System;

namespace CareSphere.Infrastructure
{
    public interface ITenantContext
    {
        Guid TenantId { get; }

        /// <summary>
        /// Allows the Blazor auth layer to explicitly inject a tenant ID
        /// when the HTTP cookie is unavailable (Interactive Server mode).
        /// </summary>
        void SetTenantId(Guid tenantId);
    }
}
