using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareSphere.Modules.Shared.ReadModels
{
    public interface IPrescriptionReadModel
    {
        Task<PrescriptionSummary?> GetSummaryAsync(Guid prescriptionId, Guid tenantId);
        Task<IEnumerable<PrescriptionSummary>> SearchActiveAsync(string query, Guid tenantId);
    }

    public class PrescriptionSummary
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<PrescriptionItemSummary> Items { get; set; } = new();

        // Additional properties for Dispense Page refactoring
        public string DrugName => Items.FirstOrDefault()?.DrugName ?? string.Empty;
        public int Quantity => Items.FirstOrDefault()?.Quantity ?? 0;
        public string DrugCode { get; set; } = string.Empty;
        public string Form { get; set; } = string.Empty;
        public string Strength { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
    }

    public class PrescriptionItemSummary
    {
        public string DrugName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Dosage { get; set; } = string.Empty;
    }
}
