using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("notification_templates")]
    public class NotificationTemplate : BaseEntity
    {

        [Required]
        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [MaxLength(150)]
        [Column("template_name")]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("notification_type")]
        public string NotificationType { get; set; } = string.Empty; // AppointmentReminder, FollowUpReminder, DischargeNotification, LabReportReady, GeneralAlert

        [Required]
        [MaxLength(50)]
        [Column("channel")]
        public string Channel { get; set; } = string.Empty; // SMS, WhatsApp, Voice, InApp

        [Required]
        [MaxLength(10)]
        [Column("language")]
        public string Language { get; set; } = "en"; // en, hi, ta, te, kn, ml, bn, mr, gu, pa

        [Required]
        [Column("template_body")]
        public string TemplateBody { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
