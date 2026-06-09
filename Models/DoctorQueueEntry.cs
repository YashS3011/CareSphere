using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("doctor_queue_entries")]
    public class DoctorQueueEntry
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("doctor_id")]
        public Guid DoctorId { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Waiting"; // Waiting | InConsultation | Completed | NoShow

        [Column("queue_position")]
        public int QueuePosition { get; set; }

        [Column("estimated_wait_minutes")]
        public int? EstimatedWaitMinutes { get; set; }

        [Required]
        [Column("checked_in_at")]
        public DateTime CheckedInAt { get; set; } = DateTime.UtcNow;

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        // Navigation properties
        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; } = null!;

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;
    }
}
