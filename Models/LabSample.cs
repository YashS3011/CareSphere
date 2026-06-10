using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("lab_samples")]
    public class LabSample : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("requisition_id")]
        public Guid RequisitionId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("sample_type")]
        public string SampleType { get; set; } = string.Empty;

        [Column("collected_at")]
        public DateTime CollectedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        [Column("collected_by_user_id")]
        public string CollectedByUserId { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("barcode_label")]
        public string? BarcodeLabel { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_received")]
        public bool IsReceived { get; set; } = false;

        [Column("received_at")]
        public DateTime? ReceivedAt { get; set; }

        [MaxLength(100)]
        [Column("received_by_user_id")]
        public string? ReceivedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("RequisitionId")]
        public LabRequisition Requisition { get; set; } = null!;
    }
}
