using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("invoice_documents")]
    public class InvoiceDocument : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [Column("invoice_id")]
        public Guid InvoiceId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("document_type")]
        public string DocumentType { get; set; } = string.Empty; // Invoice, Receipt, DischargeSummaryBill, ClaimBundle

        [MaxLength(512)]
        [Column("storage_path")]
        public string? StoragePath { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [Column("generated_at")]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [Column("file_size_bytes")]
        public long? FileSizeBytes { get; set; }

        [Required]
        [Column("is_latest")]
        public bool IsLatest { get; set; } = true;

        // Navigation properties
        [ForeignKey("InvoiceId")]
        public BillingInvoice BillingInvoice { get; set; } = null!;
    }
}
