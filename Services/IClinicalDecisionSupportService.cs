// FR-016: Clinical Decision Support - Recommended Feature
// This service provides drug interaction checking capabilities.
// It queries the DrugInteractions table for matches between existing prescriptions
// and a newly selected drug.

using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IClinicalDecisionSupportService
    {
        Task<List<DrugInteractionAlert>> CheckDrugInteractionsAsync(List<string> existingDrugCodes, string newDrugCode);
    }

    public class DrugInteractionAlert
    {
        public string Severity { get; set; } = string.Empty; // Advisory | Warning | Contraindicated
        public string AlertMessage { get; set; } = string.Empty;
        public string InteractingDrug { get; set; } = string.Empty;
    }
}
