using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("service_bus_outbox")]
    public class ServiceBusOutbox : BaseEntity
    {

        [Required]
        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [MaxLength(100)]
        [Column("message_type")]
        public string MessageType { get; set; } = string.Empty;

        [Required]
        [Column("payload")]
        public string Payload { get; set; } = string.Empty; // JSON format

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Pending"; // Pending, Enqueued, Processed, Failed

        [Column("scheduled_enqueue_at")]
        public DateTime? ScheduledEnqueueAt { get; set; }

        [Column("enqueued_at")]
        public DateTime? EnqueuedAt { get; set; }

        [Column("processed_at")]
        public DateTime? ProcessedAt { get; set; }

        [Column("failure_reason")]
        public string? FailureReason { get; set; }

        [MaxLength(100)]
        [Column("service_bus_message_id")]
        public string? ServiceBusMessageId { get; set; }
    }
}
