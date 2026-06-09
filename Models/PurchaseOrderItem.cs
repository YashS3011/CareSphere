using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("purchase_order_items")]
    public class PurchaseOrderItem
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [Column("po_id")]
        public Guid PoId { get; set; }

        [Required]
        [Column("item_id")]
        public Guid ItemId { get; set; }

        [Column("requested_quantity")]
        public int RequestedQuantity { get; set; }

        [Column("received_quantity")]
        public int ReceivedQuantity { get; set; } = 0;

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [Column("total_price")]
        public decimal TotalPrice { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("PoId")]
        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        [ForeignKey("ItemId")]
        public PharmacyItem Item { get; set; } = null!;
    }
}
