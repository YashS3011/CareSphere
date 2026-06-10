using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("grn_items")]
    public class GrnItem : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [Column("grn_id")]
        public Guid GrnId { get; set; }

        [Required]
        [Column("item_id")]
        public Guid ItemId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("batch_number")]
        public string BatchNumber { get; set; } = string.Empty;

        [Column("manufacture_date")]
        public DateTime? ManufactureDate { get; set; }

        [Required]
        [Column("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        [Column("received_quantity")]
        public int ReceivedQuantity { get; set; }

        [Column("free_quantity")]
        public int FreeQuantity { get; set; } = 0;

        [Column("purchase_price")]
        public decimal PurchasePrice { get; set; }

        [Column("selling_price")]
        public decimal SellingPrice { get; set; }

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("batch_id")]
        public Guid? BatchId { get; set; } // Set after posting

        // Navigation properties
        [ForeignKey("GrnId")]
        public GoodsReceivedNote Grn { get; set; } = null!;

        [ForeignKey("ItemId")]
        public PharmacyItem Item { get; set; } = null!;

        [ForeignKey("BatchId")]
        public PharmacyBatch? Batch { get; set; }
    }
}
