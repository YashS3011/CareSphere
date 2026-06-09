using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("stock_ledger")]
    public class StockLedgerEntry
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [Column("item_id")]
        public Guid ItemId { get; set; }

        [Column("batch_id")]
        public Guid? BatchId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("transaction_type")]
        public string TransactionType { get; set; } = string.Empty; // GRN, Dispensed, ReturnedToSupplier, Adjustment, OtcSale, Expired

        [MaxLength(100)]
        [Column("reference_id")]
        public string? ReferenceId { get; set; }

        [MaxLength(50)]
        [Column("reference_type")]
        public string? ReferenceType { get; set; } // GRN, Dispense, OtcSale

        [Column("quantity_in")]
        public int QuantityIn { get; set; } = 0;

        [Column("quantity_out")]
        public int QuantityOut { get; set; } = 0;

        [Column("balance_after")]
        public int BalanceAfter { get; set; }

        [Required]
        [Column("transaction_date")]
        public DateTime TransactionDate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("created_by_user_id")]
        public string CreatedByUserId { get; set; } = "system";

        // Navigation properties
        [ForeignKey("ItemId")]
        public PharmacyItem Item { get; set; } = null!;

        [ForeignKey("BatchId")]
        public PharmacyBatch? Batch { get; set; }
    }
}
