using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("lab_reports")]
    public class LabReport : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("requisition_id")]
        public Guid RequisitionId { get; set; }

        [Column("generated_at")]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        [Column("generated_by_user_id")]
        public string GeneratedByUserId { get; set; } = string.Empty;

        [Column("storage_path")]
        public string? StoragePath { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Column("is_latest")]
        public bool IsLatest { get; set; } = true;

        [Column("notification_sent_at")]
        public DateTime? NotificationSentAt { get; set; }

        [MaxLength(50)]
        [Column("patient_sms_status")]
        public string? PatientSmsStatus { get; set; } // Sent, Failed, Skipped

        [MaxLength(50)]
        [Column("doctor_sms_status")]
        public string? DoctorSmsStatus { get; set; } // Sent, Failed, Skipped

        [Column("in_app_notification_sent")]
        public bool InAppNotificationSent { get; set; } = false;

        // Navigation property
        [ForeignKey("RequisitionId")]
        public LabRequisition Requisition { get; set; } = null!;
    }
}
