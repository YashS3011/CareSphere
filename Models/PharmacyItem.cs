using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("pharmacy_items")]
    public class PharmacyItem : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [MaxLength(50)]
        [Column("item_code")]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("item_name")]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("generic_name")]
        public string? GenericName { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("category")]
        public string Category { get; set; } = "Medicine"; // Medicine, Surgical, Consumable, Equipment, Other

        [MaxLength(50)]
        [Column("form")]
        public string? Form { get; set; } // Tablet, Capsule, Syrup, Injection, Cream

        [MaxLength(50)]
        [Column("strength")]
        public string? Strength { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("unit")]
        public string Unit { get; set; } = "Strip"; // Strip, Box, Vial, Bottle

        [MaxLength(100)]
        [Column("barcode")]
        public string? Barcode { get; set; }

        [Column("is_controlled")]
        public bool IsControlled { get; set; } = false;

        [Column("requires_prescription")]
        public bool RequiresPrescription { get; set; } = true;

        [Column("reorder_level")]
        public int ReorderLevel { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
