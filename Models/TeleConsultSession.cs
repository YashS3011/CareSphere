using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("tele_consult_sessions")]
    public class TeleConsultSession : BaseEntity
    {

        [Required]
        [Column("encounter_id")]
        public Guid EncounterId { get; set; }

        [Required]
        [Column("doctor_id")]
        public Guid DoctorId { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("session_type")]
        public string SessionType { get; set; } = "Consultation"; // Consultation | ICU

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Scheduled"; // Scheduled | Active | Ended

        [MaxLength(500)]
        [Column("meeting_link")]
        public string? MeetingLink { get; set; }

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("ended_at")]
        public DateTime? EndedAt { get; set; }

        [Column("duration_minutes")]
        public int? DurationMinutes { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        // Navigation properties
        [ForeignKey("EncounterId")]
        public Encounter Encounter { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; } = null!;

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;
    }
}
