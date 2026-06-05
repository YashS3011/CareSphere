using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("beds")]
    public class Bed
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

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

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("WardId")]
        public Ward Ward { get; set; } = null!;
        public ICollection<BedAllotment> Allotments { get; set; } = new List<BedAllotment>();
    }
}
