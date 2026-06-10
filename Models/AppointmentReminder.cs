using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("appointment_reminders")]
    public class AppointmentReminder : BaseEntity
    {

        [Required]
        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("appointment_id")]
        public Guid? AppointmentId { get; set; } // Nullable because appointments module is built later

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Column("doctor_id")]
        public Guid? DoctorId { get; set; }

        [Required]
        [Column("appointment_date")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("reminder_type")]
        public string ReminderType { get; set; } = string.Empty; // TwentyFourHour, TwoHour, FollowUp

        [Required]
        [MaxLength(50)]
        [Column("channel")]
        public string Channel { get; set; } = string.Empty; // SMS, WhatsApp, Voice, InApp

        [Required]
        [MaxLength(10)]
        [Column("language")]
        public string Language { get; set; } = "en";

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Sent, Cancelled

        [Required]
        [Column("scheduled_at")]
        public DateTime ScheduledAt { get; set; }

        [Column("sent_at")]
        public DateTime? SentAt { get; set; }

        [Column("notification_log_id")]
        public Guid? NotificationLogId { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }

        [ForeignKey("NotificationLogId")]
        public NotificationLog? NotificationLog { get; set; }
    }
}
