using System;

namespace CareSphere.Infrastructure
{
    public class BypassTenantContext : ITenantContext
    {
        public Guid TenantId { get; }

        public BypassTenantContext(Guid tenantId)
        {
            TenantId = tenantId;
        }

        // No-op: this context is used for seeding/migrations where tenant ID is fixed at construction.
        public void SetTenantId(Guid tenantId) { }
    }
}
