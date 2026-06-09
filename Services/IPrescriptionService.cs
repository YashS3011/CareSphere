using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IPrescriptionService
    {
        Task<Prescription> CreatePrescriptionAsync(Prescription prescription);
        Task<List<Prescription>> GetPrescriptionsByEncounterAsync(Guid encounterId);
        Task CancelPrescriptionAsync(Guid prescriptionId, string reason);
        Task<List<DrugFormulary>> SearchDrugFormularyAsync(string searchTerm);
    }
}
