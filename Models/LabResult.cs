using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("lab_results")]
    public class LabResult : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("requisition_item_id")]
        public Guid RequisitionItemId { get; set; }

        [Column("parameter_id")]
        public Guid ParameterId { get; set; }

        [Required]
        [Column("result_value")]
        public string ResultValue { get; set; } = string.Empty;

        [Column("result_numeric")]
        public decimal? ResultNumeric { get; set; }

        [MaxLength(50)]
        [Column("result_unit")]
        public string? ResultUnit { get; set; }

        [Column("reference_range_low")]
        public decimal? ReferenceRangeLow { get; set; }

        [Column("reference_range_high")]
        public decimal? ReferenceRangeHigh { get; set; }

        [Column("reference_range_text")]
        public string? ReferenceRangeText { get; set; }

        [Column("is_abnormal")]
        public bool IsAbnormal { get; set; } = false;

        [MaxLength(10)]
        [Column("abnormal_flag")]
        public string? AbnormalFlag { get; set; } // H, L, HH, LL, A

        [Required]
        [MaxLength(100)]
        [Column("entered_by_user_id")]
        public string EnteredByUserId { get; set; } = string.Empty;

        [Column("entered_at")]
        public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        [Column("verified_by_user_id")]
        public string? VerifiedByUserId { get; set; }

        [Column("verified_at")]
        public DateTime? VerifiedAt { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("fhir_observation_json")]
        public string? FhirObservationJson { get; set; }

        [MaxLength(100)]
        [Column("fhir_observation_id")]
        public string? FhirObservationId { get; set; }

        // Navigation properties
        [ForeignKey("RequisitionItemId")]
        public LabRequisitionItem RequisitionItem { get; set; } = null!;

        [ForeignKey("ParameterId")]
        public LabTestParameter Parameter { get; set; } = null!;
    }
}
