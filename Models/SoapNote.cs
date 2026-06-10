using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("soap_notes")]
    public class SoapNote : BaseEntity
    {

        [Required]
        [Column("encounter_id")]
        public Guid EncounterId { get; set; }

        [Required]
        [Column("subjective")]
        public string Subjective { get; set; } = string.Empty;

        [Required]
        [Column("objective")]
        public string Objective { get; set; } = string.Empty;

        [Required]
        [Column("assessment")]
        public string Assessment { get; set; } = string.Empty;

        [Required]
        [Column("plan")]
        public string Plan { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Draft"; // Draft | Final

        [Column("finalized_at")]
        public DateTime? FinalizedAt { get; set; }

        [Required]
        [Column("created_by_doctor_id")]
        public Guid CreatedByDoctorId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        // Navigation properties
        [ForeignKey("EncounterId")]
        public Encounter Encounter { get; set; } = null!;

        [ForeignKey("CreatedByDoctorId")]
        public Doctor CreatedByDoctor { get; set; } = null!;
    }
}
