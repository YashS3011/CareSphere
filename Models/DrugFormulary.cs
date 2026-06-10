using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("drug_formulary")]
    public class DrugFormulary : BaseEntity
    {

        [Required]
        [MaxLength(50)]
        [Column("drug_code")]
        public string DrugCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("generic_name")]
        public string GenericName { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("brand_name")]
        public string? BrandName { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("form")]
        public string Form { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("strength")]
        public string Strength { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("unit")]
        public string Unit { get; set; } = string.Empty;

        [Column("is_controlled")]
        public bool IsControlled { get; set; } = false;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;
    }
}
