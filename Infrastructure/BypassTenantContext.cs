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
    }
}
