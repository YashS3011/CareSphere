using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class BedService : IBedService
    {
        private readonly ApplicationDbContext _context;

        public BedService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Ward Methods ---
        public async Task<List<Ward>> GetAllWardsAsync()
        {
            return await _context.Wards
                .Include(w => w.Beds)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<Ward?> GetWardByIdAsync(Guid id)
        {
            return await _context.Wards
                .Include(w => w.Beds)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Ward> CreateWardAsync(Ward ward)
        {
            ward.Id = Guid.NewGuid();
            ward.CreatedAt = DateTime.UtcNow;

            _context.Wards.Add(ward);
            await _context.SaveChangesAsync();
            return ward;
        }

        public async Task<Ward> UpdateWardAsync(Ward ward)
        {
            var existing = await _context.Wards.FindAsync(ward.Id);
            if (existing != null)
            {
                existing.Name = ward.Name;
                existing.WardType = ward.WardType;
                existing.Floor = ward.Floor;
                existing.Building = ward.Building;

                _context.Wards.Update(existing);
                await _context.SaveChangesAsync();
            }
            return existing ?? ward;
        }

        public async Task<bool> DeleteWardAsync(Guid id)
        {
            // Business Rule: Ward deletion only allowed if no beds exist under it.
            var hasBeds = await _context.Beds.AnyAsync(b => b.WardId == id);
            if (hasBeds)
            {
                throw new InvalidOperationException("Cannot delete a ward that contains beds. Please remove or reassign the beds first.");
            }

            var ward = await _context.Wards.FindAsync(id);
            if (ward != null)
            {
                _context.Wards.Remove(ward);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // --- Bed Methods ---
        public async Task<List<Bed>> GetAllBedsAsync(string? wardId = null, string? status = null)
        {
            var query = _context.Beds.Include(b => b.Ward).AsQueryable();

            if (!string.IsNullOrEmpty(wardId) && Guid.TryParse(wardId, out var wId))
            {
                query = query.Where(b => b.WardId == wId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            return await query.OrderBy(b => b.BedNumber).ToListAsync();
        }

        public async Task<Bed?> GetBedByIdAsync(Guid id)
        {
            return await _context.Beds
                .Include(b => b.Ward)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Bed> CreateBedAsync(Bed bed)
        {
            bed.Id = Guid.NewGuid();
            bed.CreatedAt = DateTime.UtcNow;

            _context.Beds.Add(bed);
            await _context.SaveChangesAsync();
            return bed;
        }

        public async Task<Bed> UpdateBedAsync(Bed bed)
        {
            var existing = await _context.Beds.FindAsync(bed.Id);
            if (existing != null)
            {
                existing.BedNumber = bed.BedNumber;
                existing.WardId = bed.WardId;
                existing.BedType = bed.BedType;
                existing.Status = bed.Status;
                existing.IsActive = bed.IsActive;

                _context.Beds.Update(existing);
                await _context.SaveChangesAsync();
            }
            return existing ?? bed;
        }

        public async Task<bool> DeleteBedAsync(Guid id)
        {
            // Business Rule: Bed deletion only allowed if bed has no active allotment
            var hasActiveAllotments = await _context.BedAllotments.AnyAsync(a => a.BedId == id && a.Status == "Active");
            if (hasActiveAllotments)
            {
                throw new InvalidOperationException("Cannot delete a bed that has an active patient allotment.");
            }

            var bed = await _context.Beds.FindAsync(id);
            if (bed != null)
            {
                _context.Beds.Remove(bed);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<Bed>> GetAvailableBedsAsync()
        {
            return await _context.Beds
                .Include(b => b.Ward)
                .Where(b => b.Status == "Available" && b.IsActive)
                .OrderBy(b => b.Ward.Name)
                .ThenBy(b => b.BedNumber)
                .ToListAsync();
        }

        // --- Allotment Methods ---
        public async Task<BedAllotment> AdmitPatientAsync(BedAllotment allotment)
        {
            // Business Rule: A bed with status Occupied or Maintenance cannot be allotted
            var bed = await _context.Beds.FindAsync(allotment.BedId);
            if (bed == null || bed.Status == "Occupied" || bed.Status == "Maintenance")
            {
                throw new InvalidOperationException("Selected bed is not available for allotment.");
            }

            // Business Rule: A patient can only have ONE active allotment at a time
            var activeAllotment = await _context.BedAllotments
                .AnyAsync(a => a.PatientId == allotment.PatientId && a.Status == "Active");
            if (activeAllotment)
            {
                throw new InvalidOperationException("Patient already has an active bed allotment.");
            }

            allotment.Id = Guid.NewGuid();
            allotment.CreatedAt = DateTime.UtcNow;
            allotment.Status = "Active";

            _context.BedAllotments.Add(allotment);

            // On allotment -> bed status changes to Occupied
            bed.Status = "Occupied";
            _context.Beds.Update(bed);

            await _context.SaveChangesAsync();
            return allotment;
        }

        public async Task<BedAllotment?> GetActiveAllotmentByBedAsync(Guid bedId)
        {
            return await _context.BedAllotments
                .Include(a => a.Patient)
                .Include(a => a.Bed)
                .ThenInclude(b => b.Ward)
                .FirstOrDefaultAsync(a => a.BedId == bedId && a.Status == "Active");
        }

        public async Task<List<BedAllotment>> GetAllotmentsByPatientAsync(Guid patientId)
        {
            return await _context.BedAllotments
                .Include(a => a.Bed)
                .ThenInclude(b => b.Ward)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AdmissionDate)
                .ToListAsync();
        }

        public async Task DischargePatientAsync(Guid allotmentId, string dischargeNotes, DateTime dischargeDate)
        {
            var allotment = await _context.BedAllotments.FindAsync(allotmentId);
            if (allotment == null || allotment.Status != "Active")
                throw new InvalidOperationException("Invalid or inactive allotment.");

            // Output logic: On Discharge: allotment status -> Discharged, bed status -> Available
            allotment.Status = "Discharged";
            allotment.DischargeNotes = dischargeNotes;
            allotment.DischargeDate = dischargeDate;
            _context.BedAllotments.Update(allotment);

            var bed = await _context.Beds.FindAsync(allotment.BedId);
            if (bed != null)
            {
                bed.Status = "Available";
                _context.Beds.Update(bed);
            }

            await _context.SaveChangesAsync();
        }

        public async Task TransferPatientAsync(Guid allotmentId, Guid newBedId, string reason)
        {
            var currentAllotment = await _context.BedAllotments.FindAsync(allotmentId);
            if (currentAllotment == null || currentAllotment.Status != "Active")
                throw new InvalidOperationException("Invalid or inactive allotment.");

            var newBed = await _context.Beds.FindAsync(newBedId);
            if (newBed == null || newBed.Status == "Occupied" || newBed.Status == "Maintenance")
                throw new InvalidOperationException("Target bed is not available for transfer.");

            var oldBed = await _context.Beds.FindAsync(currentAllotment.BedId);

            // Create transfer record
            var transfer = new BedTransfer
            {
                Id = Guid.NewGuid(),
                AllotmentId = currentAllotment.Id,
                FromBedId = currentAllotment.BedId,
                ToBedId = newBedId,
                TransferReason = reason,
                TransferredAt = DateTime.UtcNow
            };
            _context.BedTransfers.Add(transfer);

            // Update old allotment status -> Transferred
            currentAllotment.Status = "Transferred";
            currentAllotment.DischargeDate = DateTime.UtcNow;
            _context.BedAllotments.Update(currentAllotment);

            // Create new active allotment
            var newAllotment = new BedAllotment
            {
                Id = Guid.NewGuid(),
                BedId = newBedId,
                PatientId = currentAllotment.PatientId,
                AdmissionDate = DateTime.UtcNow,
                AdmissionType = currentAllotment.AdmissionType,
                AdmittingDoctor = currentAllotment.AdmittingDoctor,
                Notes = currentAllotment.Notes + "\n\n[Transferred from " + (oldBed?.BedNumber ?? "unknown") + "]",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            _context.BedAllotments.Add(newAllotment);

            // Update bed statuses
            if (oldBed != null)
            {
                oldBed.Status = "Available";
                _context.Beds.Update(oldBed);
            }
            newBed.Status = "Occupied";
            _context.Beds.Update(newBed);

            await _context.SaveChangesAsync();
        }

        // --- Dashboard Stats ---
        public async Task<BedDashboardStats> GetDashboardStatsAsync()
        {
            var beds = await _context.Beds.Include(b => b.Ward).ToListAsync();

            var stats = new BedDashboardStats
            {
                TotalBeds = beds.Count,
                Available = beds.Count(b => b.Status == "Available" && b.IsActive),
                Occupied = beds.Count(b => b.Status == "Occupied"),
                Maintenance = beds.Count(b => b.Status == "Maintenance"),
                Reserved = beds.Count(b => b.Status == "Reserved")
            };

            var wardGroups = beds.GroupBy(b => b.Ward).Where(g => g.Key != null);
            foreach (var group in wardGroups)
            {
                stats.WardBreakdown.Add(new WardSummary
                {
                    WardName = group.Key.Name,
                    WardType = group.Key.WardType,
                    Total = group.Count(),
                    Occupied = group.Count(b => b.Status == "Occupied"),
                    Available = group.Count(b => b.Status == "Available" && b.IsActive)
                });
            }

            return stats;
        }
    }
}
