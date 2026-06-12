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
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Modules.Notifications.Services
{
    public interface IDischargeNotificationService
    {
        Task SendDischargeNotificationAsync(Guid tenantId, Guid patientId, Guid? allotmentId, DateTime dischargedAt, string language = "en");
        Task<List<DischargeNotification>> GetDischargeNotificationsByPatientAsync(Guid patientId);
    }
}
