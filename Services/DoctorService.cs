using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;

        public DoctorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetAllDoctorsAsync(string? search = null)
        {
            var query = _context.Doctors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(d =>
                    d.FullName.ToLower().Contains(search) ||
                    d.Specialization.ToLower().Contains(search));
            }

            return await query.OrderBy(d => d.FullName).ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(Guid id)
        {
            return await _context.Doctors.FindAsync(id);
        }

        public async Task CreateDoctorAsync(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDoctorAsync(Doctor doctor)
        {
            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDoctorAsync(Guid id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                // Soft check: Do not delete if doctor has existing encounters
                var hasEncounters = await _context.Encounters.AnyAsync(e => e.DoctorId == id);
                if (hasEncounters)
                {
                    throw new InvalidOperationException("Cannot delete doctor with existing encounters. Please deactivate instead.");
                }

                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Doctor>> GetActiveDoctorsAsync()
        {
            return await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.FullName)
                .ToListAsync();
        }
    }
}
