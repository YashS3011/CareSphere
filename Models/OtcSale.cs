using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("otc_sales")]
    public class OtcSale : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [MaxLength(50)]
        [Column("sale_number")]
        public string SaleNumber { get; set; } = string.Empty; // Auto-generated: OTC-YYYYMMDD-XXXX

        [Required]
        [Column("sale_date")]
        public DateTime SaleDate { get; set; }

        [MaxLength(200)]
        [Column("customer_name")]
        public string? CustomerName { get; set; }

        [MaxLength(30)]
        [Column("customer_phone")]
        public string? CustomerPhone { get; set; }

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("payment_method")]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, UPI, Card

        [MaxLength(100)]
        [Column("razorpay_order_id")]
        public string? RazorpayOrderId { get; set; }

        [MaxLength(100)]
        [Column("razorpay_payment_id")]
        public string? RazorpayPaymentId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("payment_status")]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed

        [Required]
        [MaxLength(100)]
        [Column("created_by_user_id")]
        public string CreatedByUserId { get; set; } = "system";

        // Navigation properties
        public ICollection<OtcSaleItem> Items { get; set; } = new List<OtcSaleItem>();
    }
}
