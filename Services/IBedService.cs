using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IBedService
    {
        Task<List<Ward>> GetAllWardsAsync();
        Task<Ward?> GetWardByIdAsync(Guid id);
        Task<Ward> CreateWardAsync(Ward ward);
        Task<Ward> UpdateWardAsync(Ward ward);
        Task<bool> DeleteWardAsync(Guid id);

        Task<List<Bed>> GetAllBedsAsync(string? wardId = null, string? status = null);
        Task<Bed?> GetBedByIdAsync(Guid id);
        Task<Bed> CreateBedAsync(Bed bed);
        Task<Bed> UpdateBedAsync(Bed bed);
        Task<bool> DeleteBedAsync(Guid id);
        Task<List<Bed>> GetAvailableBedsAsync();

        Task<BedAllotment> AdmitPatientAsync(BedAllotment allotment);
        Task<BedAllotment?> GetActiveAllotmentByBedAsync(Guid bedId);
        Task<List<BedAllotment>> GetAllotmentsByPatientAsync(Guid patientId);
        Task DischargePatientAsync(Guid allotmentId, string dischargeNotes, DateTime dischargeDate);
        Task TransferPatientAsync(Guid allotmentId, Guid newBedId, string reason);

        Task<BedDashboardStats> GetDashboardStatsAsync();
    }
}
