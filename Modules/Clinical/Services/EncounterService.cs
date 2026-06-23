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
using System.Text.Json;
using CareSphere.Modules.Shared.Events;

namespace CareSphere.Modules.Clinical.Services
{
    public class EncounterService : IEncounterService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationSenderService _notificationSender;

        public EncounterService(ApplicationDbContext context, INotificationSenderService notificationSender)
        {
            _context = context;
            _notificationSender = notificationSender;
        }

        public async Task<Encounter> CreateEncounterAsync(Encounter encounter)
        {
            encounter.Id = Guid.NewGuid();
            encounter.CreatedAt = DateTime.UtcNow;

            _context.Encounters.Add(encounter);
            await _context.SaveChangesAsync();
            return encounter;
        }

        public async Task<List<Encounter>> GetEncountersByPatientAsync(Guid patientId)
        {
            return await _context.Encounters.AsNoTracking()
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.AdmissionDate)
                .ToListAsync();
        }

        public async Task<Encounter?> GetEncounterByIdAsync(Guid id)
        {
            return await _context.Encounters.AsNoTracking()
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task UpdateEncounterStatusAsync(Guid id, string status)
        {
            var encounter = await _context.Encounters.FindAsync(id);
            if (encounter == null)
                throw new InvalidOperationException("Encounter not found.");

            encounter.Status = status;

            if (status == "Completed")
            {
                encounter.DischargeDate = DateTime.UtcNow;

                var outboxEvent = new EncounterCompleted
                {
                    EncounterId = encounter.Id,
                    PatientId = encounter.PatientId,
                    TenantId = encounter.TenantId
                };

                var outbox = new ServiceBusOutbox
                {
                    Id = Guid.NewGuid(),
                    TenantId = encounter.TenantId,
                    MessageType = "EncounterCompleted",
                    Payload = JsonSerializer.Serialize(outboxEvent),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.ServiceBusOutboxes.Add(outbox);
            }

            _context.Encounters.Update(encounter);
            await _context.SaveChangesAsync();
        }

        public async Task<Encounter?> GetActiveEncounterForPatientAsync(Guid patientId)
        {
            return await _context.Encounters.AsNoTracking()
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .FirstOrDefaultAsync(e => e.PatientId == patientId &&
                                          (e.Status == "Planned" || e.Status == "InProgress"));
        }

        public async Task<List<Encounter>> GetAllEncountersAsync(string? searchTerm = null, string? statusFilter = null, string? typeFilter = null)
        {
            var query = _context.Encounters.AsNoTracking()
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(e => e.Patient.FirstName.ToLower().Contains(lower) ||
                                         e.Patient.LastName.ToLower().Contains(lower) ||
                                         e.Patient.Mrn.ToLower().Contains(lower));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(e => e.Status == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                query = query.Where(e => e.EncounterType == typeFilter);
            }

            return await query.OrderByDescending(e => e.AdmissionDate).ToListAsync();
        }

        public async Task SetDispositionAsync(Guid encounterId, string disposition)
        {
            var encounter = await _context.Encounters.FindAsync(encounterId);
            if (encounter == null)
                throw new InvalidOperationException("Encounter not found.");

            encounter.DischargeDisposition = disposition;
            _context.Encounters.Update(encounter);
            await _context.SaveChangesAsync();

            if (disposition == "IPD")
            {
                var users = await _context.Users
                    .Where(u => u.TenantId == encounter.TenantId &&
                                (u.Role == CareSphereRoles.FrontDesk || u.Role == CareSphereRoles.HospitalAdmin) &&
                                u.IsActive)
                    .ToListAsync();

                var patient = await _context.Patients.FindAsync(encounter.PatientId);
                var patientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "Patient";
                var mrn = patient?.Mrn ?? "";

                foreach (var user in users)
                {
                    await _notificationSender.SendInAppAsync(
                        tenantId: encounter.TenantId,
                        recipientType: "User",
                        recipientId: user.Id,
                        title: "Patient Admission Required",
                        messageBody: $"Admission required for patient {patientName} (MRN: {mrn}) from encounter.",
                        resourceType: "Encounter",
                        resourceId: encounter.Id.ToString(),
                        patientId: encounter.PatientId,
                        notificationType: "PatientAdmissionRequired"
                    );
                }
            }
        }

        public async Task AddDiagnosisAsync(Guid encounterId, string icdCode, string icdDescription, string diagnosisType)
        {
            var encounter = await _context.Encounters.FindAsync(encounterId);
            if (encounter == null)
                throw new InvalidOperationException("Encounter not found.");

            var diagnosis = new EncounterDiagnosis
            {
                Id = Guid.NewGuid(),
                TenantId = encounter.TenantId,
                EncounterId = encounterId,
                IcdCode = icdCode,
                IcdDescription = icdDescription,
                DiagnosisType = diagnosisType,
                CreatedAt = DateTime.UtcNow
            };

            _context.EncounterDiagnoses.Add(diagnosis);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EncounterDiagnosis>> GetDiagnosesAsync(Guid encounterId)
        {
            return await _context.EncounterDiagnoses
                .AsNoTracking()
                .Where(d => d.EncounterId == encounterId)
                .OrderBy(d => d.CreatedAt)
                .ToListAsync();
        }
    }
}
