using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("billing_invoices")]
    public class BillingInvoice : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("invoice_number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Column("encounter_id")]
        public Guid? EncounterId { get; set; }

        [Column("admission_date")]
        public DateTime? AdmissionDate { get; set; }

        [Column("discharge_date")]
        public DateTime? DischargeDate { get; set; }

        [Required]
        [Column("invoice_date")]
        public DateTime InvoiceDate { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Required]
        [Column("subtotal_amount")]
        public decimal SubtotalAmount { get; set; }

        [Column("discount_amount")]
        public decimal DiscountAmount { get; set; } = 0m;

        [Column("tax_amount")]
        public decimal TaxAmount { get; set; } = 0m;

        [Required]
        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("paid_amount")]
        public decimal PaidAmount { get; set; } = 0m;

        [Column("balance_amount")]
        public decimal BalanceAmount { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Draft"; // Draft, Finalized, PartiallyPaid, Paid, Cancelled

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("generated_by_user_id")]
        public string GeneratedByUserId { get; set; } = "system";

        [Required]

        // Navigation properties
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("EncounterId")]
        public Encounter? Encounter { get; set; }

        public ICollection<BillingLineItem> BillingLineItems { get; set; } = new List<BillingLineItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
