using System;
using System.Threading.Tasks;

namespace CareSphere.Services
{
    public interface IServiceBusService
    {
        Task EnqueueMessageAsync(string messageType, object payload, Guid tenantId, DateTime? scheduledEnqueueAt = null);
        Task ProcessOutboxAsync();
    }
}
