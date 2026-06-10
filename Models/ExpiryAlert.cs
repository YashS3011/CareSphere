using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("expiry_alerts")]
    public class ExpiryAlert : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [Column("batch_id")]
        public Guid BatchId { get; set; }

        [Required]
        [Column("item_id")]
        public Guid ItemId { get; set; }

        [Required]
        [Column("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        [Column("days_until_expiry")]
        public int DaysUntilExpiry { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("alert_type")]
        public string AlertType { get; set; } = "InApp"; // InApp, SMS

        [Required]
        [Column("sent_at")]
        public DateTime SentAt { get; set; }

        [Column("is_acknowledged")]
        public bool IsAcknowledged { get; set; } = false;

        [Column("acknowledged_at")]
        public DateTime? AcknowledgedAt { get; set; }

        [MaxLength(100)]
        [Column("acknowledged_by_user_id")]
        public string? AcknowledgedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("BatchId")]
        public PharmacyBatch Batch { get; set; } = null!;

        [ForeignKey("ItemId")]
        public PharmacyItem Item { get; set; } = null!;
    }
}
