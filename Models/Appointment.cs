using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("appointments")]
    public class Appointment : BaseEntity
    {
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Column("doctor_id")]
        public Guid DoctorId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Column("slot_start")]
        public DateTime SlotStart { get; set; }

        [Column("slot_end")]
        public DateTime SlotEnd { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("appointment_type")]
        public string AppointmentType { get; set; } = string.Empty;  // "OPD", "FollowUp", "TeleConsult"

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = string.Empty;            // "Scheduled", "Confirmed", "Cancelled", "Completed", "NoShow"

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("booked_by_user_id")]
        public string BookedByUserId { get; set; } = string.Empty;

        // Navigation
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; } = null!;
    }
}
