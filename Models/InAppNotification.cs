using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("in_app_notifications")]
    public class InAppNotification
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [MaxLength(50)]
        [Column("recipient_type")]
        public string RecipientType { get; set; } = string.Empty; // Doctor, Patient

        [Required]
        [MaxLength(100)]
        [Column("recipient_id")]
        public string RecipientId { get; set; } = string.Empty; // doctor UUID or patient UUID as string

        [Required]
        [MaxLength(200)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("resource_type")]
        public string ResourceType { get; set; } = string.Empty; // e.g. LabReport

        [Required]
        [MaxLength(100)]
        [Column("resource_id")]
        public string ResourceId { get; set; } = string.Empty;

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }
    }
}
