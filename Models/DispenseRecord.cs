using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("dispense_records")]
    public class DispenseRecord : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [Column("prescription_id")]
        public Guid PrescriptionId { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Required]
        [Column("item_id")]
        public Guid ItemId { get; set; }

        [Required]
        [Column("batch_id")]
        public Guid BatchId { get; set; }

        [Column("dispensed_quantity")]
        public int DispensedQuantity { get; set; }

        [Required]
        [Column("dispensed_at")]
        public DateTime DispensedAt { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("dispensed_by_user_id")]
        public string DispensedByUserId { get; set; } = "system";

        [MaxLength(100)]
        [Column("barcode_scanned")]
        public string? BarcodeScanned { get; set; }

        [Column("is_partial_dispense")]
        public bool IsPartialDispense { get; set; } = false;

        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("PrescriptionId")]
        public Prescription Prescription { get; set; } = null!;

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("ItemId")]
        public PharmacyItem Item { get; set; } = null!;

        [ForeignKey("BatchId")]
        public PharmacyBatch Batch { get; set; } = null!;
    }
}
