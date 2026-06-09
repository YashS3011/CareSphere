using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("otc_sale_items")]
    public class OtcSaleItem
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [Column("sale_id")]
        public Guid SaleId { get; set; }

        [Required]
        [Column("item_id")]
        public Guid ItemId { get; set; }

        [Required]
        [Column("batch_id")]
        public Guid BatchId { get; set; }

        [MaxLength(100)]
        [Column("barcode_scanned")]
        public string? BarcodeScanned { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [Column("total_price")]
        public decimal TotalPrice { get; set; }

        // Navigation properties
        [ForeignKey("SaleId")]
        public OtcSale Sale { get; set; } = null!;

        [ForeignKey("ItemId")]
        public PharmacyItem Item { get; set; } = null!;

        [ForeignKey("BatchId")]
        public PharmacyBatch Batch { get; set; } = null!;
    }
}
