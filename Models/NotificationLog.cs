using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("notification_logs")]
    public class NotificationLog : BaseEntity
    {

        [Required]
        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("patient_id")]
        public Guid? PatientId { get; set; }

        [Column("doctor_id")]
        public Guid? DoctorId { get; set; }

        [MaxLength(50)]
        [Column("recipient_phone")]
        public string? RecipientPhone { get; set; }

        [MaxLength(150)]
        [Column("recipient_name")]
        public string? RecipientName { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("channel")]
        public string Channel { get; set; } = string.Empty; // SMS, WhatsApp, Voice, InApp

        [Required]
        [MaxLength(100)]
        [Column("notification_type")]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [Column("language")]
        public string Language { get; set; } = "en";

        [Required]
        [Column("message_body")]
        public string MessageBody { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Pending"; // Pending, Sent, Failed, Delivered, Skipped

        [MaxLength(100)]
        [Column("provider_message_id")]
        public string? ProviderMessageId { get; set; } // Twilio message SID / Call SID

        [Column("provider_response")]
        public string? ProviderResponse { get; set; }

        [Column("scheduled_at")]
        public DateTime? ScheduledAt { get; set; }

        [Column("sent_at")]
        public DateTime? SentAt { get; set; }

        [Column("delivered_at")]
        public DateTime? DeliveredAt { get; set; }

        [Column("failure_reason")]
        public string? FailureReason { get; set; }

        [Column("retry_count")]
        public int RetryCount { get; set; } = 0;

        [Column("max_retries")]
        public int MaxRetries { get; set; } = 3;

        [MaxLength(100)]
        [Column("service_bus_message_id")]
        public string? ServiceBusMessageId { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }
    }
}
