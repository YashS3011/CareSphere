using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("goods_received_notes")]
    public class GoodsReceivedNote : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [MaxLength(50)]
        [Column("grn_number")]
        public string GrnNumber { get; set; } = string.Empty; // Auto-generated: GRN-YYYYMMDD-XXXX

        [Column("po_id")]
        public Guid? PoId { get; set; }

        [Required]
        [Column("supplier_id")]
        public Guid SupplierId { get; set; }

        [Required]
        [Column("received_date")]
        public DateTime ReceivedDate { get; set; }

        [MaxLength(100)]
        [Column("invoice_number")]
        public string? InvoiceNumber { get; set; }

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Draft"; // Draft, Posted

        [Required]
        [MaxLength(100)]
        [Column("received_by_user_id")]
        public string ReceivedByUserId { get; set; } = "system";

        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("PoId")]
        public PurchaseOrder? PurchaseOrder { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; } = null!;

        public ICollection<GrnItem> Items { get; set; } = new List<GrnItem>();
    }
}
