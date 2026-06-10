using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("discharge_notifications")]
    public class DischargeNotification : BaseEntity
    {

        [Required]
        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("allotment_id")]
        public Guid? AllotmentId { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Required]
        [Column("discharged_at")]
        public DateTime DischargedAt { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("channel")]
        public string Channel { get; set; } = string.Empty; // SMS, WhatsApp, InApp

        [Required]
        [MaxLength(10)]
        [Column("language")]
        public string Language { get; set; } = "en";

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Pending"; // Pending, Sent, Failed

        [Column("notification_log_id")]
        public Guid? NotificationLogId { get; set; }

        // Navigation properties
        [ForeignKey("AllotmentId")]
        public BedAllotment? BedAllotment { get; set; }

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("NotificationLogId")]
        public NotificationLog? NotificationLog { get; set; }
    }
}
