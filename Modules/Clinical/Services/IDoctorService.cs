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
using CareSphere.Models;

namespace CareSphere.Modules.Clinical.Services
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
