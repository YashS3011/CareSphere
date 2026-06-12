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

namespace CareSphere.Modules.Clinical.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;

        public DoctorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetAllDoctorsAsync(string? searchTerm = null)
        {
            var query = _context.Doctors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(d => d.FirstName.ToLower().Contains(lower) ||
                                         d.LastName.ToLower().Contains(lower) ||
                                         d.Specialization.ToLower().Contains(lower));
            }

            return await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
        }

        public async Task<List<Doctor>> GetActiveDoctorsAsync()
        {
            return await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(Guid id)
        {
            return await _context.Doctors.FindAsync(id);
        }

        public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
        {
            doctor.Id = Guid.NewGuid();
            doctor.CreatedAt = DateTime.UtcNow;

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return doctor;
        }

        public async Task<Doctor> UpdateDoctorAsync(Doctor doctor)
        {
            var existing = await _context.Doctors.FindAsync(doctor.Id);
            if (existing != null)
            {
                existing.FirstName = doctor.FirstName;
                existing.LastName = doctor.LastName;
                existing.Specialization = doctor.Specialization;
                existing.RegistrationNumber = doctor.RegistrationNumber;
                existing.Phone = doctor.Phone;
                existing.Email = doctor.Email;
                existing.IsActive = doctor.IsActive;

                _context.Doctors.Update(existing);
                await _context.SaveChangesAsync();
            }
            return existing ?? doctor;
        }

        public async Task<bool> DeleteDoctorAsync(Guid id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
