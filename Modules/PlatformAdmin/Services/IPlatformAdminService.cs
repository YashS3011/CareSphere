using CareSphere.Models;

namespace CareSphere.Modules.PlatformAdmin.Services
{
    /// <summary>
    /// Summary stats for a single hospital tenant.
    /// </summary>
    public class HospitalClientStats
    {
        public int TotalUsers    { get; set; }
        public int ActiveUsers   { get; set; }
        public int TotalPatients { get; set; }
        public int TotalBeds     { get; set; }
        public string SubscriptionTier { get; set; } = "Free";
    }

    public interface IPlatformAdminService
    {
        /// <summary>Returns all tenant settings (bypasses tenant filter).</summary>
        Task<List<TenantSettings>> GetAllHospitalsAsync();

        /// <summary>Creates a new tenant and its default HospitalAdmin user.</summary>
        Task<(TenantSettings Tenant, string AdminEmail, string AdminPassword)> CreateHospitalAsync(
            string hospitalName, string? city, string? state, string? phone, string? email,
            string subscriptionTier, int maxUsers, string? addressLine1);

        /// <summary>Freezes (deactivates) all user accounts of the specified tenant.</summary>
        Task FreezeHospitalUsersAsync(Guid tenantId);

        /// <summary>Unfreezes (activates) all user accounts of the specified tenant.</summary>
        Task UnfreezeHospitalUsersAsync(Guid tenantId);

        /// <summary>Hard-deletes all records for the tenant — irreversible.</summary>
        Task DeleteHospitalAsync(Guid tenantId);

        /// <summary>Gets summary statistics for a single tenant.</summary>
        Task<HospitalClientStats> GetHospitalStatsAsync(Guid tenantId);

        /// <summary>Updates hospital tenant configuration settings.</summary>
        Task UpdateHospitalSettingsAsync(TenantSettings updatedSettings);
    }
}
