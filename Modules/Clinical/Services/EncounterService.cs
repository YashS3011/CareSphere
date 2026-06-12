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

        public EncounterService(ApplicationDbContext context)
        {
            _context = context;
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
            return await _context.Encounters
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.AdmissionDate)
                .ToListAsync();
        }

        public async Task<Encounter?> GetEncounterByIdAsync(Guid id)
        {
            return await _context.Encounters
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
            return await _context.Encounters
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .FirstOrDefaultAsync(e => e.PatientId == patientId &&
                                          (e.Status == "Planned" || e.Status == "InProgress"));
        }

        public async Task<List<Encounter>> GetAllEncountersAsync(string? searchTerm = null, string? statusFilter = null, string? typeFilter = null)
        {
            var query = _context.Encounters
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
    }
}
