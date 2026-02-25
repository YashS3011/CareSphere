using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IDoctorService
    {
        Task<List<Doctor>> GetAllDoctorsAsync(string? search = null);
        Task<Doctor?> GetDoctorByIdAsync(Guid id);
        Task CreateDoctorAsync(Doctor doctor);
        Task UpdateDoctorAsync(Doctor doctor);
        Task DeleteDoctorAsync(Guid id);
        Task<List<Doctor>> GetActiveDoctorsAsync();
    }
}
