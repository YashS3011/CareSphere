using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;

namespace CareSphere.Modules.Shared.ReadModels
{
    public class PrescriptionReadModel : IPrescriptionReadModel
    {
        private readonly ApplicationDbContext _dbContext;

        public PrescriptionReadModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PrescriptionSummary?> GetSummaryAsync(Guid prescriptionId, Guid tenantId)
        {
            var rx = await _dbContext.Prescriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == prescriptionId);

            if (rx == null) return null;

            return new PrescriptionSummary
            {
                Id = rx.Id,
                PatientId = rx.PatientId,
                Status = rx.Status,
                DrugCode = rx.DrugCode ?? string.Empty,
                Form = rx.Form,
                Strength = rx.Strength,
                IssuedAt = rx.IssuedAt,
                Items = new List<PrescriptionItemSummary>
                {
                    new PrescriptionItemSummary
                    {
                        DrugName = rx.DrugName,
                        Quantity = rx.Quantity,
                        Dosage = $"{rx.Strength} {rx.Form} - {rx.Frequency} ({rx.Duration})"
                    }
                }
            };
        }

        public async Task<IEnumerable<PrescriptionSummary>> SearchActiveAsync(string query, Guid tenantId)
        {
            var queryable = _dbContext.Prescriptions
                .AsNoTracking()
                .Where(p => p.Status == "Active");

            if (Guid.TryParse(query, out var rxId))
            {
                queryable = queryable.Where(p => p.Id == rxId);
            }
            else
            {
                var cleanQuery = query.Trim().ToLower();
                // Match Patient MRN or Name
                var patientIds = await _dbContext.Patients
                    .AsNoTracking()
                    .Where(p => p.Mrn.ToLower() == cleanQuery || 
                                (p.FirstName + " " + p.LastName).ToLower().Contains(cleanQuery))
                    .Select(p => p.Id)
                    .ToListAsync();

                queryable = queryable.Where(p => patientIds.Contains(p.PatientId));
            }

            var prescriptions = await queryable
                .OrderByDescending(p => p.IssuedAt)
                .ToListAsync();

            return prescriptions.Select(rx => new PrescriptionSummary
            {
                Id = rx.Id,
                PatientId = rx.PatientId,
                Status = rx.Status,
                DrugCode = rx.DrugCode ?? string.Empty,
                Form = rx.Form,
                Strength = rx.Strength,
                IssuedAt = rx.IssuedAt,
                Items = new List<PrescriptionItemSummary>
                {
                    new PrescriptionItemSummary
                    {
                        DrugName = rx.DrugName,
                        Quantity = rx.Quantity,
                        Dosage = $"{rx.Strength} {rx.Form} - {rx.Frequency} ({rx.Duration})"
                    }
                }
            }).ToList();
        }
    }
}
