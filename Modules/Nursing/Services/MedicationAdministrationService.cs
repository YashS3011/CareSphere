using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;

namespace CareSphere.Modules.Nursing.Services
{
    public class MedicationAdministrationService : IMedicationAdministrationService
    {
        private readonly ApplicationDbContext _dbContext;

        public MedicationAdministrationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MedicationAdministrationRecord> RecordAdministrationAsync(
            MedicationAdministrationRecord mar, Guid tenantId)
        {
            mar.Id = Guid.NewGuid();
            mar.TenantId = tenantId;
            if (mar.AdministeredAt == default)
            {
                mar.AdministeredAt = DateTime.UtcNow;
            }
            mar.CreatedAt = DateTime.UtcNow;

            _dbContext.MedicationAdministrationRecords.Add(mar);
            await _dbContext.SaveChangesAsync();
            return mar;
        }

        public async Task<List<MedicationAdministrationRecord>> GetByPatientAsync(
            Guid patientId, Guid tenantId)
        {
            return await _dbContext.MedicationAdministrationRecords
                .Include(m => m.Prescription)
                .Where(m => m.PatientId == patientId && m.TenantId == tenantId)
                .OrderByDescending(m => m.AdministeredAt)
                .ToListAsync();
        }

        public async Task<List<MedicationAdministrationRecord>> GetByPrescriptionAsync(
            Guid prescriptionId, Guid tenantId)
        {
            return await _dbContext.MedicationAdministrationRecords
                .Where(m => m.PrescriptionId == prescriptionId && m.TenantId == tenantId)
                .OrderByDescending(m => m.AdministeredAt)
                .ToListAsync();
        }
    }
}
