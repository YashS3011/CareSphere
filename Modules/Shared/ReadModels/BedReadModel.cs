using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;

namespace CareSphere.Modules.Shared.ReadModels
{
    public class BedReadModel : IBedReadModel
    {
        private readonly ApplicationDbContext _dbContext;

        public BedReadModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BedSummary?> GetCurrentAllotmentAsync(Guid patientId, Guid tenantId)
        {
            var allotment = await _dbContext.BedAllotments
                .AsNoTracking()
                .Include(a => a.Bed)
                .ThenInclude(b => b.Ward)
                .Where(a => a.PatientId == patientId && a.Status == "Active")
                .OrderByDescending(a => a.AdmissionDate)
                .FirstOrDefaultAsync();

            if (allotment == null) return null;

            return new BedSummary
            {
                BedId = allotment.BedId,
                BedNumber = allotment.Bed.BedNumber,
                WardName = allotment.Bed.Ward.Name,
                Status = allotment.Status
            };
        }
    }
}
