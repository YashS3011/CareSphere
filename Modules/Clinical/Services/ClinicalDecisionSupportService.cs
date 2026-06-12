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
// This service checks for drug-drug interactions by querying the DrugInteractions table.
// It matches the new drug code against existing prescribed drug codes in both directions
// (DrugCodeA↔DrugCodeB) to find any known interactions.

using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Modules.Clinical.Services
{
    public class ClinicalDecisionSupportService : IClinicalDecisionSupportService
    {
        private readonly ApplicationDbContext _context;

        public ClinicalDecisionSupportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DrugInteractionAlert>> CheckDrugInteractionsAsync(List<string> existingDrugCodes, string newDrugCode)
        {
            if (string.IsNullOrWhiteSpace(newDrugCode) || !existingDrugCodes.Any())
                return new List<DrugInteractionAlert>();

            // Query interactions where new drug matches either side
            var interactions = await _context.DrugInteractions
                .Where(di =>
                    (di.DrugCodeA == newDrugCode && existingDrugCodes.Contains(di.DrugCodeB)) ||
                    (di.DrugCodeB == newDrugCode && existingDrugCodes.Contains(di.DrugCodeA)))
                .ToListAsync();

            var alerts = new List<DrugInteractionAlert>();

            foreach (var interaction in interactions)
            {
                var interactingCode = interaction.DrugCodeA == newDrugCode
                    ? interaction.DrugCodeB
                    : interaction.DrugCodeA;

                // Look up drug name from formulary for a better alert message
                var drug = await _context.DrugFormulary
                    .FirstOrDefaultAsync(d => d.DrugCode == interactingCode);

                var drugName = drug?.GenericName ?? interactingCode;

                alerts.Add(new DrugInteractionAlert
                {
                    Severity = interaction.Severity,
                    AlertMessage = interaction.Description ?? $"{interaction.Severity} interaction between {newDrugCode} and {interactingCode}.",
                    InteractingDrug = drugName
                });
            }

            return alerts;
        }
    }
}
