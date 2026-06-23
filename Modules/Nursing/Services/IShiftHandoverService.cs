using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Modules.Nursing.Services
{
    public interface IShiftHandoverService
    {
        Task<ShiftHandover> AddHandoverAsync(ShiftHandover handover, Guid tenantId);
        Task<List<ShiftHandover>> GetByPatientAsync(Guid patientId, Guid tenantId);
        Task<List<ShiftHandover>> GetLatestHandoversAsync(Guid tenantId, int limit = 10);
    }
}
