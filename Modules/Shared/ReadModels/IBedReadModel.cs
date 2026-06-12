using System;
using System.Threading.Tasks;

namespace CareSphere.Modules.Shared.ReadModels
{
    public interface IBedReadModel
    {
        Task<BedSummary?> GetCurrentAllotmentAsync(Guid patientId, Guid tenantId);
    }

    public class BedSummary
    {
        public Guid BedId { get; set; }
        public string BedNumber { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
