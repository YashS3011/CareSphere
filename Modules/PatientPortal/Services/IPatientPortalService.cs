using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Modules.PatientPortal.Services
{
    public class PatientPortalData
    {
        public List<Prescription> Prescriptions        { get; set; } = new();
        public List<LabRequisition> LabRequisitions    { get; set; } = new();
        public List<BillingInvoice> Invoices           { get; set; } = new();
        public List<LabReport> LabReports              { get; set; } = new();
        public List<Appointment> Appointments          { get; set; } = new();
        public List<MedicationAdministrationRecord> MARs { get; set; } = new();
        public List<NursingNote> NursingNotes          { get; set; } = new();
        public Models.Patient? PatientInfo             { get; set; }
    }

    public interface IPatientPortalService
    {
        Task<Models.Patient?> GetPatientInfoAsync(Guid patientId);
        Task<List<Prescription>> GetMyPrescriptionsAsync(Guid patientId);
        Task<List<LabRequisition>> GetMyLabRequisitionsAsync(Guid patientId);
        Task<List<LabReport>> GetMyLabReportsAsync(Guid patientId);
        Task<List<BillingInvoice>> GetMyInvoicesAsync(Guid patientId);
        Task<List<Appointment>> GetMyAppointmentsAsync(Guid patientId);
        Task<List<MedicationAdministrationRecord>> GetMyMARsAsync(Guid patientId);
        Task<List<NursingNote>> GetMyNursingNotesAsync(Guid patientId);
    }
}
