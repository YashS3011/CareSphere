using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IQueueService
    {
        Task<DoctorQueueEntry> AddToQueueAsync(DoctorQueueEntry entry);
        Task UpdateStatusAsync(Guid entryId, string newStatus);
        Task<List<DoctorQueueEntry>> GetQueueForDoctorAsync(Guid doctorId);
        Task<int> CalculateEtaAsync(Guid doctorId);
    }
}
