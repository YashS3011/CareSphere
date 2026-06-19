using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;

namespace CareSphere.Modules.Shared.ReadModels
{
    public class DoctorReadModel : IDoctorReadModel
    {
        private readonly ApplicationDbContext _dbContext;

        public DoctorReadModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DoctorSummary?> GetSummaryAsync(Guid doctorId, Guid tenantId)
        {
            var doctor = await _dbContext.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.TenantId == tenantId);

            if (doctor == null) return null;

            return new DoctorSummary
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Specialty = doctor.Specialization,
                RegistrationNumber = doctor.RegistrationNumber
            };
        }

        public async Task<IEnumerable<DoctorSummary>> GetAllAsync(Guid tenantId)
        {
            var doctors = await _dbContext.Doctors
                .AsNoTracking()
                .Where(d => d.TenantId == tenantId && d.IsActive)
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .ToListAsync();

            return doctors.Select(d => new DoctorSummary
            {
                Id = d.Id,
                FullName = d.FullName,
                Specialty = d.Specialization,
                RegistrationNumber = d.RegistrationNumber
            }).ToList();
        }
    }
}
