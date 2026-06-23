using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;

namespace CareSphere.Modules.Nursing.Services
{
    public class ShiftHandoverService : IShiftHandoverService
    {
        private readonly ApplicationDbContext _dbContext;

        public ShiftHandoverService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ShiftHandover> AddHandoverAsync(ShiftHandover handover, Guid tenantId)
        {
            handover.Id = Guid.NewGuid();
            handover.TenantId = tenantId;
            if (handover.HandoverDate == default)
            {
                handover.HandoverDate = DateTime.UtcNow;
            }
            handover.CreatedAt = DateTime.UtcNow;

            _dbContext.ShiftHandovers.Add(handover);
            await _dbContext.SaveChangesAsync();
            return handover;
        }

        public async Task<List<ShiftHandover>> GetByPatientAsync(Guid patientId, Guid tenantId)
        {
            return await _dbContext.ShiftHandovers.AsNoTracking()
                .Include(s => s.Patient)
                .Where(s => s.PatientId == patientId && s.TenantId == tenantId)
                .OrderByDescending(s => s.HandoverDate)
                .ToListAsync();
        }

        public async Task<List<ShiftHandover>> GetLatestHandoversAsync(Guid tenantId, int limit = 10)
        {
            return await _dbContext.ShiftHandovers.AsNoTracking()
                .Include(s => s.Patient)
                .Where(s => s.TenantId == tenantId)
                .OrderByDescending(s => s.HandoverDate)
                .Take(limit)
                .ToListAsync();
        }
    }
}
