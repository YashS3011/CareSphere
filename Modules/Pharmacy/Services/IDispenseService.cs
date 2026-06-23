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
using CareSphere.Models;

namespace CareSphere.Modules.Pharmacy.Services
{
    public interface IDispenseService
    {
        /// <summary>
        /// FR-026: Database-layer prescription authorization enforcement.
        /// Validates if a prescription is valid, matches the drug code, is active,
        /// and checks the remaining quantity available for dispensing.
        /// </summary>
        Task<DispenseValidationResult> ValidatePrescriptionForDispenseAsync(Guid prescriptionId, string? barcodeScanned = null);

        Task<List<DispenseRecord>> DispenseItemAsync(Guid prescriptionId, int quantityToDispense, string dispensedByUserId, string? barcodeScanned = null, string? witnessUserId = null);

        Task<List<DispenseRecord>> GetDispenseHistoryByPrescriptionAsync(Guid prescriptionId);
        Task<List<DispenseRecord>> GetDispenseHistoryByPatientAsync(Guid patientId);
    }
}
