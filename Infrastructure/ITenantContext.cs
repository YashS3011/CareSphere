using System;

namespace CareSphere.Infrastructure
{
    public interface ITenantContext
    {
        Guid TenantId { get; }
    }
}
