using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IDischargeNotificationService
    {
        Task SendDischargeNotificationAsync(Guid tenantId, Guid patientId, Guid? allotmentId, DateTime dischargedAt, string language = "en");
        Task<List<DischargeNotification>> GetDischargeNotificationsByPatientAsync(Guid patientId);
    }
}
