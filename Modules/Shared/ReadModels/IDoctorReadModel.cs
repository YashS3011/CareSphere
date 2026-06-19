using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareSphere.Modules.Shared.ReadModels
{
    public interface IDoctorReadModel
    {
        Task<DoctorSummary?> GetSummaryAsync(Guid doctorId, Guid tenantId);
        Task<IEnumerable<DoctorSummary>> GetAllAsync(Guid tenantId);
    }

    public class DoctorSummary
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
    }
}
