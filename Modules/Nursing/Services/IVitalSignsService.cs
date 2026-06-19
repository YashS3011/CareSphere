using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Modules.Nursing.Services
{
    public interface IVitalSignsService
    {
        Task<VitalSigns> RecordAsync(VitalSigns vitals, Guid tenantId);
        Task<List<VitalSigns>> GetHistoryAsync(Guid patientId, Guid tenantId, int lastN = 10);
        Task<VitalSigns?> GetLatestAsync(Guid patientId, Guid tenantId);
    }
}
