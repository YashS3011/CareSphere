using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    /// <summary>
    /// Tracks active and revoked user sessions for multi-device management and forced logout.
    /// </summary>
    [Table("user_sessions")]
    public class UserSession : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        /// <summary>Foreign key to AspNetUsers.Id</summary>
        [Required]
        [MaxLength(450)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("session_token")]
        public string SessionToken { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [MaxLength(200)]
        [Column("device_info")]
        public string? DeviceInfo { get; set; }

        [MaxLength(500)]
        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("last_activity_at")]
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_revoked")]
        public bool IsRevoked { get; set; } = false;

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }
    }
}
