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
// FR-016: Clinical Decision Support - Recommended Feature
// This service provides drug interaction checking capabilities.
// It queries the DrugInteractions table for matches between existing prescriptions
// and a newly selected drug.

using CareSphere.Models;

namespace CareSphere.Modules.Clinical.Services
{
    public interface IClinicalDecisionSupportService
    {
        Task<List<DrugInteractionAlert>> CheckDrugInteractionsAsync(List<string> existingDrugCodes, string newDrugCode);
        Task<List<AllergyAlert>> CheckAllergiesAsync(Guid patientId, string drugCode);
    }

    public class DrugInteractionAlert
    {
        public string Severity { get; set; } = string.Empty; // Advisory | Warning | Contraindicated
        public string AlertMessage { get; set; } = string.Empty;
        public string InteractingDrug { get; set; } = string.Empty;
    }

    public class AllergyAlert
    {
        public string Allergen { get; set; } = string.Empty;
        public string AlertMessage { get; set; } = string.Empty;
    }
}
