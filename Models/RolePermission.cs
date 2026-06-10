using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    /// <summary>
    /// Per-tenant role permission override stored in the database.
    /// These override or supplement the defaults in RolePermissionDefaults.
    /// </summary>
    [Table("role_permissions")]
    public class RolePermission : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("role_name")]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("permission")]
        public string Permission { get; set; } = string.Empty;
    }
}
