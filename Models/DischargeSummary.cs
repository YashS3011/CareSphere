using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("discharge_summaries")]
    public class DischargeSummary
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("encounter_id")]
        public Guid EncounterId { get; set; }

        [ForeignKey("EncounterId")]
        public Encounter? Encounter { get; set; }

        [Required]
        [Column("admission_date")]
        public DateTime AdmissionDate { get; set; }

        [Required]
        [Column("discharge_date")]
        public DateTime DischargeDate { get; set; }

        [Required]
        [Column("final_diagnosis")]
        public string FinalDiagnosis { get; set; } = string.Empty;

        [Required]
        [Column("treatment_summary")]
        public string TreatmentSummary { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("condition_at_discharge")]
        public string ConditionAtDischarge { get; set; } = string.Empty; // Stable | Improved | Critical | LAMA | Expired

        [Column("followup_instructions")]
        public string? FollowupInstructions { get; set; }

        [Required]
        [Column("authorized_by_doctor_id")]
        public Guid AuthorizedByDoctorId { get; set; }

        [ForeignKey("AuthorizedByDoctorId")]
        public Doctor? AuthorizedByDoctor { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
