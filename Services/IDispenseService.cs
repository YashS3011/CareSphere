using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IDispenseService
    {
        /// <summary>
        /// FR-026: Database-layer prescription authorization enforcement.
        /// Validates if a prescription is valid, matches the drug code, is active,
        /// and checks the remaining quantity available for dispensing.
        /// </summary>
        Task<DispenseValidationResult> ValidatePrescriptionForDispenseAsync(Guid prescriptionId, string? barcodeScanned = null);

        Task<List<DispenseRecord>> DispenseItemAsync(Guid prescriptionId, int quantityToDispense, string dispensedByUserId, string? barcodeScanned = null);

        Task<List<DispenseRecord>> GetDispenseHistoryByPrescriptionAsync(Guid prescriptionId);
        Task<List<DispenseRecord>> GetDispenseHistoryByPatientAsync(Guid patientId);
    }
}
