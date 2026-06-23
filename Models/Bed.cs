using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("beds")]
    public class Bed : BaseEntity
    {

        [Required]
        [MaxLength(50)]
        [Column("bed_number")]
        public string BedNumber { get; set; } = string.Empty;

        [Required]
        [Column("ward_id")]
        public Guid WardId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("bed_type")]
        public string BedType { get; set; } = string.Empty; // Standard | ICU | Isolation | Pediatric

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Available"; // Available | Occupied | Maintenance | Reserved

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("daily_charge_amount")]
        public decimal DailyChargeAmount { get; set; } = 0.0m;

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        // Navigation properties
        [ForeignKey("WardId")]
        public Ward Ward { get; set; } = null!;
        public ICollection<BedAllotment> Allotments { get; set; } = new List<BedAllotment>();
    }
}
