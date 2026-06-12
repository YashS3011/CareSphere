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
using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Modules.Clinical.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public PrescriptionService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<Prescription> CreatePrescriptionAsync(Prescription prescription)
        {
            prescription.Id = Guid.NewGuid();
            prescription.IssuedAt = DateTime.UtcNow;
            prescription.Status = "Active";

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();
            return prescription;
        }

        public async Task<List<Prescription>> GetPrescriptionsByEncounterAsync(Guid encounterId)
        {
            return await _context.Prescriptions
                .Include(p => p.Doctor)
                .Where(p => p.EncounterId == encounterId)
                .OrderByDescending(p => p.IssuedAt)
                .ToListAsync();
        }

        public async Task CancelPrescriptionAsync(Guid prescriptionId, string reason)
        {
            var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
            if (prescription == null)
                throw new InvalidOperationException("Prescription not found.");

            if (prescription.Status == "Cancelled")
                throw new InvalidOperationException("Prescription is already cancelled.");

            prescription.Status = "Cancelled";
            prescription.CancelledAt = DateTime.UtcNow;
            prescription.CancellationReason = reason;

            _context.Prescriptions.Update(prescription);
            await _context.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system", // Will be replaced once auth is added
                Action = "PRESCRIPTION_CANCELLED",
                ResourceType = "Prescription",
                ResourceId = prescriptionId.ToString(),
                TenantId = prescription.TenantId
            });
        }

        public async Task<List<DrugFormulary>> SearchDrugFormularyAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<DrugFormulary>();

            var lower = searchTerm.ToLower();
            return await _context.DrugFormulary
                .Where(d => d.IsActive &&
                           (d.GenericName.ToLower().Contains(lower) ||
                            (d.BrandName != null && d.BrandName.ToLower().Contains(lower)) ||
                            d.DrugCode.ToLower().Contains(lower)))
                .OrderBy(d => d.GenericName)
                .Take(20)
                .ToListAsync();
        }
    }
}
