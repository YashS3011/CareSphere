using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("purchase_orders")]
    public class PurchaseOrder : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [MaxLength(50)]
        [Column("po_number")]
        public string PoNumber { get; set; } = string.Empty; // Auto-generated: PO-YYYYMMDD-XXXX

        [Required]
        [Column("supplier_id")]
        public Guid SupplierId { get; set; }

        [Required]
        [Column("order_date")]
        public DateTime OrderDate { get; set; }

        [Column("expected_delivery_date")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = "Draft"; // Draft, Sent, PartiallyReceived, Received, Cancelled

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("created_by_user_id")]
        public string CreatedByUserId { get; set; } = "system";

        // Navigation properties
        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; } = null!;

        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}
