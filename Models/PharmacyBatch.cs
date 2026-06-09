using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("pharmacy_batches")]
    public class PharmacyBatch
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [Column("item_id")]
        public Guid ItemId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("batch_number")]
        public string BatchNumber { get; set; } = string.Empty;

        [Column("supplier_id")]
        public Guid? SupplierId { get; set; }

        [Column("manufacture_date")]
        public DateTime? ManufactureDate { get; set; }

        [Required]
        [Column("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        [Column("purchase_price")]
        public decimal PurchasePrice { get; set; }

        [Column("selling_price")]
        public decimal SellingPrice { get; set; }

        [Column("current_stock")]
        public int CurrentStock { get; set; }

        [Column("reserved_stock")]
        public int ReservedStock { get; set; } = 0;

        [Column("available_stock")]
        public int AvailableStock { get; set; } // Computed: CurrentStock - ReservedStock

        [Column("expiry_alert_sent_at")]
        public DateTime? ExpiryAlertSentAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ItemId")]
        public PharmacyItem Item { get; set; } = null!;

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }
    }
}
