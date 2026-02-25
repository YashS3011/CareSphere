using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("opd_queue")]
    public class OpdQueue
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        [Column("doctor_id")]
        public Guid DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }

        [Required]
        [Column("queue_date", TypeName = "date")]
        public DateOnly QueueDate { get; set; }

        [Required]
        [Column("scheduled_time")]
        public TimeSpan ScheduledTime { get; set; }

        [Required]
        [Column("token_number")]
        public int TokenNumber { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("visit_type")]
        public string VisitType { get; set; } = string.Empty; // OPD | Follow-Up | Emergency

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Waiting"; // Waiting | InConsultation | Completed | NoShow

        public Encounter? Encounter { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
