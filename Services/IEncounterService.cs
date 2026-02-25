using CareSphere.Models;
using CareSphere.Models.DTOs;

namespace CareSphere.Services
{
    public interface IEncounterService
    {
        // Queue
        Task<List<OpdQueue>> GetQueueByDoctorAndDateAsync(Guid doctorId, DateOnly date);
        Task<OpdQueue> AddToQueueAsync(OpdQueue entry);
        Task UpdateQueueStatusAsync(Guid queueId, string status);
        Task<int> GetNextTokenNumberAsync(Guid doctorId, DateOnly date);

        // Encounter
        Task<Encounter> StartEncounterAsync(Encounter encounter);
        Task<Encounter?> GetEncounterByIdAsync(Guid id);
        Task<List<Encounter>> GetEncountersByPatientAsync(Guid patientId);
        Task UpdateEncounterStatusAsync(Guid encounterId, string status);
        Task FinishEncounterAsync(Guid encounterId);

        // SOAP
        Task SaveSoapNoteAsync(SoapNote note);
        Task<SoapNote?> GetSoapNoteByEncounterAsync(Guid encounterId);

        // Diagnosis
        Task AddDiagnosisAsync(Diagnosis diagnosis);
        Task UpdateDiagnosisAsync(Diagnosis diagnosis);
        Task DeleteDiagnosisAsync(Guid id);
        Task<List<Diagnosis>> GetDiagnosesByEncounterAsync(Guid encounterId);

        // Prescription
        Task AddPrescriptionAsync(Prescription prescription);
        Task UpdatePrescriptionAsync(Prescription prescription);
        Task DeletePrescriptionAsync(Guid id);
        Task<List<Prescription>> GetPrescriptionsByEncounterAsync(Guid encounterId);
        Task<bool> CheckAllergyWarningAsync(Guid patientId, string medicineName);

        // Procedure
        Task AddProcedureAsync(Procedure procedure);
        Task<List<Procedure>> GetProceduresByEncounterAsync(Guid encounterId);

        // Discharge
        Task SaveDischargeSummaryAsync(DischargeSummary summary);
        Task<DischargeSummary?> GetDischargeSummaryByEncounterAsync(Guid encounterId);

        // Dashboard
        Task<DoctorDashboardStats> GetDoctorDashboardAsync(Guid doctorId, DateOnly date);
    }
}
