using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("insurance_claims")]
    public class InsuranceClaim : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [Column("invoice_id")]
        public Guid InvoiceId { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Column("encounter_id")]
        public Guid? EncounterId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("claim_number")]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("insurance_provider")]
        public string InsuranceProvider { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("policy_number")]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("member_name")]
        public string MemberName { get; set; } = string.Empty;

        [Required]
        [Column("claimed_amount")]
        public decimal ClaimedAmount { get; set; }

        [Column("approved_amount")]
        public decimal? ApprovedAmount { get; set; }

        [Column("rejected_amount")]
        public decimal? RejectedAmount { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, UnderReview, Approved, PartiallyApproved, Rejected, Resubmitted

        [Column("submitted_at")]
        public DateTime? SubmittedAt { get; set; }

        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        [Column("rejected_at")]
        public DateTime? RejectedAt { get; set; }

        [Column("rejection_reason")]
        public string? RejectionReason { get; set; }

        [Column("fhir_claim_bundle_json")]
        public string? FhirClaimBundleJson { get; set; }

        [MaxLength(255)]
        [Column("fhir_claim_bundle_reference")]
        public string? FhirClaimBundleReference { get; set; }

        [MaxLength(100)]
        [Column("abdm_transaction_id")]
        public string? AbdmTransactionId { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]

        // Navigation properties
        [ForeignKey("InvoiceId")]
        public BillingInvoice BillingInvoice { get; set; } = null!;

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("EncounterId")]
        public Encounter? Encounter { get; set; }

        public ICollection<ClaimStatusHistory> ClaimStatusHistories { get; set; } = new List<ClaimStatusHistory>();
    }
}
