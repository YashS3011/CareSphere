using CareSphere.Modules.Clinical.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Patients.Services;
using CareSphere.Modules.Ward.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Shared.Services;
using CareSphere.Modules.Shared.Events;
using System;
using System.Threading.Tasks;

namespace CareSphere.Modules.Shared.Services
{
    public interface IServiceBusService
    {
        Task EnqueueMessageAsync(string messageType, object payload, Guid tenantId, DateTime? scheduledEnqueueAt = null);
        Task ProcessOutboxAsync();
    }
}
