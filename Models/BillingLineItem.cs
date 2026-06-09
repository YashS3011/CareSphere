using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("billing_line_items")]
    public class BillingLineItem
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
        [MaxLength(50)]
        [Column("item_type")]
        public string ItemType { get; set; } = string.Empty; // Consultation, Procedure, Medication, RoomCharge, LabTest, Radiology, Nursing, Miscellaneous

        [Required]
        [MaxLength(255)]
        [Column("item_description")]
        public string ItemDescription { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("item_code")]
        public string? ItemCode { get; set; }

        [Required]
        [Column("quantity")]
        public decimal Quantity { get; set; } = 1m;

        [Required]
        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [Column("discount_percent")]
        public decimal DiscountPercent { get; set; } = 0m;

        [Column("tax_percent")]
        public decimal TaxPercent { get; set; } = 0m;

        [Required]
        [Column("line_total")]
        public decimal LineTotal { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("InvoiceId")]
        public BillingInvoice BillingInvoice { get; set; } = null!;
    }
}
