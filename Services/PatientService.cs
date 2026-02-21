using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _context;

        public PatientService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Patient>> GetPatientsAsync(string? searchTerm = null)
        {
            var query = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(p => p.FirstName.ToLower().Contains(lowerSearchTerm) ||
                                         p.LastName.ToLower().Contains(lowerSearchTerm) ||
                                         p.Mrn.ToLower().Contains(lowerSearchTerm));
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(Guid id)
        {
            return await _context.Patients.FindAsync(id);
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            patient.Id = Guid.NewGuid();
            patient.CreatedAt = DateTime.UtcNow;
            
            // Auto-generate MRN: MRN-YYYYMMDD-XXXX
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = new Random().Next(1000, 9999);
            patient.Mrn = $"MRN-{datePart}-{randomPart}";

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<Patient> UpdatePatientAsync(Patient patient)
        {
            var existing = await _context.Patients.FindAsync(patient.Id);
            if (existing != null)
            {
                existing.FirstName = patient.FirstName;
                existing.LastName = patient.LastName;
                existing.DateOfBirth = patient.DateOfBirth;
                existing.Gender = patient.Gender;
                existing.Phone = patient.Phone;
                existing.Email = patient.Email;
                existing.Address = patient.Address;
                existing.AbhaId = patient.AbhaId;
                existing.BloodGroup = patient.BloodGroup;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.Patients.Update(existing);
                await _context.SaveChangesAsync();
            }
            return existing ?? patient;
        }

        public async Task<bool> DeletePatientAsync(Guid id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        
        public async Task<int> GetPatientCountAsync()
        {
            return await _context.Patients.CountAsync();
        }
    }
}
