using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("controlled_substance_logs")]
    public class ControlledSubstanceLog : BaseEntity
    {
        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [Column("dispense_record_id")]
        public Guid DispenseRecordId { get; set; }

        [Required]
        [Column("item_id")]
        public Guid ItemId { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("dispensed_by_user_id")]
        public string DispensedByUserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("witness_user_id")]
        public string WitnessUserId { get; set; } = string.Empty;

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("log_date")]
        public DateTime LogDate { get; set; } = DateTime.UtcNow;

        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("DispenseRecordId")]
        public DispenseRecord DispenseRecord { get; set; } = null!;

        [ForeignKey("ItemId")]
        public PharmacyItem Item { get; set; } = null!;

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;
    }
}
