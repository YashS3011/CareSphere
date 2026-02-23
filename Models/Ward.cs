using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("wards")]
    public class Ward
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("ward_type")]
        public string WardType { get; set; } = string.Empty; // General | ICU | Emergency | Private | Semi-Private

        [MaxLength(50)]
        [Column("floor")]
        public string? Floor { get; set; }

        [MaxLength(100)]
        [Column("building")]
        public string? Building { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Bed> Beds { get; set; } = new List<Bed>();
        
        [NotMapped]
        public int TotalBeds => Beds.Count;
    }
}
