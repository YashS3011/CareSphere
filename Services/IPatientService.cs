using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IPatientService
    {
        Task<List<Patient>> GetPatientsAsync(string? searchTerm = null);
        Task<Patient?> GetPatientByIdAsync(Guid id);
        Task<Patient> CreatePatientAsync(Patient patient);
        Task<Patient> UpdatePatientAsync(Patient patient);
        Task<bool> DeletePatientAsync(Guid id);
        Task<int> GetPatientCountAsync();
    }
}
