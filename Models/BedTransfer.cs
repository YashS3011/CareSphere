using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("bed_transfers")]
    public class BedTransfer
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("allotment_id")]
        public Guid AllotmentId { get; set; }

        [Required]
        [Column("from_bed_id")]
        public Guid FromBedId { get; set; }

        [Required]
        [Column("to_bed_id")]
        public Guid ToBedId { get; set; }

        [Required]
        [Column("transfer_reason")]
        public string TransferReason { get; set; } = string.Empty;

        [Required]
        [Column("transferred_at")]
        public DateTime TransferredAt { get; set; } = DateTime.UtcNow;

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        // Navigation properties
        [ForeignKey("AllotmentId")]
        public BedAllotment Allotment { get; set; } = null!;

        [ForeignKey("FromBedId")]
        public Bed FromBed { get; set; } = null!;

        [ForeignKey("ToBedId")]
        public Bed ToBed { get; set; } = null!;
    }
}
