using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareSphere.Modules.Shared.ReadModels
{
    public interface IPatientReadModel
    {
        Task<PatientSummary?> GetSummaryAsync(Guid patientId, Guid tenantId);
        Task<IEnumerable<PatientSummary>> SearchAsync(string query, Guid tenantId);
    }

    public class PatientSummary
    {
        public Guid Id { get; set; }
        public string MRN { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PreferredChannel { get; set; } = string.Empty;
    }
}
