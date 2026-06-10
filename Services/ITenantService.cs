using CareSphere.Models;

namespace CareSphere.Services
{
    public class TenantStats
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalBeds { get; set; }
        public int AvailableBeds { get; set; }
        public string SubscriptionTier { get; set; } = "Free";
        public int TotalDoctors { get; set; }
        public int ActiveEncountersToday { get; set; }
        public int ActiveSessions { get; set; }
        public int TotalAuditEvents24h { get; set; }
        public int MaxUsersAllowed { get; set; }
        /// <summary>Maps role name → count of users with that role.</summary>
        public Dictionary<string, int> UserCountsByRole { get; set; } = new();
    }

    public interface ITenantService
    {
        Task<TenantSettings> GetTenantSettingsAsync(Guid tenantId);
        Task<TenantSettings> UpdateTenantSettingsAsync(TenantSettings settings, string updatedByUserId);
        Task<TenantSettings> ConfigureSsoAsync(Guid tenantId, bool ssoEnabled, string? provider, string? clientId, string? clientSecret, string? authority, string? samlMetadataUrl, string updatedByUserId);
        Task<TenantStats> GetTenantStatsAsync(Guid tenantId);
    }
}
