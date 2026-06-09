using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("audit_events")]
    public class AuditEvent
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("resource_type")]
        public string ResourceType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("resource_id")]
        public string ResourceId { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;
    }
}
