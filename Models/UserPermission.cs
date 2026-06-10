using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    /// <summary>
    /// Explicit permission grant or revocation for a specific user.
    /// Takes precedence over role-default permissions.
    /// </summary>
    [Table("user_permissions")]
    public class UserPermission : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        /// <summary>Foreign key to AspNetUsers.Id (string PK from Identity).</summary>
        [Required]
        [MaxLength(450)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("permission")]
        public string Permission { get; set; } = string.Empty;

        [Column("granted_at")]
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(450)]
        [Column("granted_by_user_id")]
        public string GrantedByUserId { get; set; } = string.Empty;

        [Column("is_revoked")]
        public bool IsRevoked { get; set; } = false;

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }
    }
}
