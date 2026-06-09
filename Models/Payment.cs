using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [Column("invoice_id")]
        public Guid InvoiceId { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Required]
        [Column("payment_date")]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("payment_method")]
        public string PaymentMethod { get; set; } = string.Empty; // UPI, Card, Cash, Insurance

        [MaxLength(100)]
        [Column("razorpay_order_id")]
        public string? RazorpayOrderId { get; set; }

        [MaxLength(100)]
        [Column("razorpay_payment_id")]
        public string? RazorpayPaymentId { get; set; }

        [MaxLength(255)]
        [Column("razorpay_signature")]
        public string? RazorpaySignature { get; set; }

        [MaxLength(100)]
        [Column("transaction_reference")]
        public string? TransactionReference { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed, Refunded

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("recorded_by_user_id")]
        public string RecordedByUserId { get; set; } = "system";

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("InvoiceId")]
        public BillingInvoice BillingInvoice { get; set; } = null!;

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;
    }
}
