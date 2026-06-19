using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;

namespace CareSphere.Modules.Shared.ReadModels
{
    public class PatientReadModel : IPatientReadModel
    {
        private readonly ApplicationDbContext _dbContext;

        public PatientReadModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PatientSummary?> GetSummaryAsync(Guid patientId, Guid tenantId)
        {
            var patient = await _dbContext.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null) return null;

            var pref = await _dbContext.PatientPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            return new PatientSummary
            {
                Id = patient.Id,
                MRN = patient.Mrn,
                FullName = $"{patient.FirstName} {patient.LastName}",
                Phone = patient.Phone,
                PreferredChannel = pref?.PreferredChannel ?? "SMS"
            };
        }

        public async Task<IEnumerable<PatientSummary>> SearchAsync(string query, Guid tenantId)
        {
            List<Patient> patients;

            if (string.IsNullOrWhiteSpace(query))
            {
                patients = await _dbContext.Patients
                    .AsNoTracking()
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(100)
                    .ToListAsync();
            }
            else
            {
                var cleanQuery = query.Trim().ToLower();
                patients = await _dbContext.Patients
                    .AsNoTracking()
                    .Where(p => p.Mrn.ToLower() == cleanQuery || 
                                (p.FirstName + " " + p.LastName).ToLower().Contains(cleanQuery) ||
                                p.Phone.Contains(cleanQuery))
                    .ToListAsync();
            }

            var patientIds = patients.Select(p => p.Id).ToList();

            var prefs = await _dbContext.PatientPreferences
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.PatientId))
                .ToDictionaryAsync(p => p.PatientId);

            return patients.Select(p => new PatientSummary
            {
                Id = p.Id,
                MRN = p.Mrn,
                FullName = $"{p.FirstName} {p.LastName}",
                Phone = p.Phone,
                PreferredChannel = prefs.TryGetValue(p.Id, out var pref) ? pref.PreferredChannel : "SMS"
            });
        }
    }
}
