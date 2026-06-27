using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Modules.PatientPortal.Services
{
    /// <summary>
    /// Patient self-service portal service.
    /// Fetches data scoped to a specific patient, using the tenant-filtered DB context.
    /// </summary>
    public class PatientPortalService : IPatientPortalService
    {
        private readonly ApplicationDbContext _context;

        public PatientPortalService(ApplicationDbContext context) => _context = context;

        public async Task<Models.Patient?> GetPatientInfoAsync(Guid patientId)
            => await _context.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == patientId);

        public async Task<List<Prescription>> GetMyPrescriptionsAsync(Guid patientId)
            => await _context.Prescriptions.AsNoTracking()
                .Include(p => p.Doctor)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.IssuedAt)
                .ToListAsync();

        public async Task<List<LabRequisition>> GetMyLabRequisitionsAsync(Guid patientId)
            => await _context.LabRequisitions.AsNoTracking()
                .Include(r => r.OrderedByDoctor)
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.OrderedAt)
                .ToListAsync();

        public async Task<List<LabReport>> GetMyLabReportsAsync(Guid patientId)
            => await _context.LabReports.AsNoTracking()
                .Include(r => r.Requisition)
                .Where(r => r.Requisition.PatientId == patientId)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync();

        public async Task<List<BillingInvoice>> GetMyInvoicesAsync(Guid patientId)
            => await _context.BillingInvoices.AsNoTracking()
                .Where(i => i.PatientId == patientId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

        public async Task<List<Appointment>> GetMyAppointmentsAsync(Guid patientId)
            => await _context.Appointments.AsNoTracking()
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.SlotStart)
                .ToListAsync();

        public async Task<List<MedicationAdministrationRecord>> GetMyMARsAsync(Guid patientId)
            => await _context.MedicationAdministrationRecords.AsNoTracking()
                .Include(m => m.Prescription)
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.AdministeredAt)
                .ToListAsync();

        public async Task<List<NursingNote>> GetMyNursingNotesAsync(Guid patientId)
            => await _context.NursingNotes.AsNoTracking()
                .Where(n => n.PatientId == patientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
    }
}
