using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IDoctorService
    {
        Task<List<Doctor>> GetAllDoctorsAsync(string? searchTerm = null);
        Task<List<Doctor>> GetActiveDoctorsAsync();
        Task<Doctor?> GetDoctorByIdAsync(Guid id);
        Task<Doctor> CreateDoctorAsync(Doctor doctor);
        Task<Doctor> UpdateDoctorAsync(Doctor doctor);
        Task<bool> DeleteDoctorAsync(Guid id);
    }
}
