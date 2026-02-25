using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("diagnoses")]
    public class Diagnosis
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("encounter_id")]
        public Guid EncounterId { get; set; }

        [ForeignKey("EncounterId")]
        public Encounter? Encounter { get; set; }

        [MaxLength(20)]
        [Column("icd10_code")]
        public string? Icd10Code { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("diagnosis_name")]
        public string DiagnosisName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("diagnosis_type")]
        public string DiagnosisType { get; set; } = string.Empty; // Primary | Secondary | Differential

        [Required]
        [MaxLength(50)]
        [Column("clinical_status")]
        public string ClinicalStatus { get; set; } = string.Empty; // Active | Resolved | Inactive

        [Column("notes")]
        public string? Notes { get; set; }

        [MaxLength(100)]
        [Column("fhir_condition_id")]
        public string? FhirConditionId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
