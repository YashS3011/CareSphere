using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("encounter_diagnoses")]
    public class EncounterDiagnosis : BaseEntity
    {
        [Required]
        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [Column("encounter_id")]
        public Guid EncounterId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("icd_code")]
        public string IcdCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("icd_description")]
        public string IcdDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("diagnosis_type")]
        public string DiagnosisType { get; set; } = "Secondary"; // Primary | Secondary

        // Navigation properties
        [ForeignKey("EncounterId")]
        public Encounter Encounter { get; set; } = null!;
    }
}
