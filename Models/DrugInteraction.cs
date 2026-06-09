using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("drug_interactions")]
    public class DrugInteraction
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("drug_code_a")]
        public string DrugCodeA { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("drug_code_b")]
        public string DrugCodeB { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("severity")]
        public string Severity { get; set; } = string.Empty; // Advisory | Warning | Contraindicated

        [Column("description")]
        public string? Description { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;
    }
}
