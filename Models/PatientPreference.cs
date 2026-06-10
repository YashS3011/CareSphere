using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("patient_preferences")]
    public class PatientPreference : BaseEntity
    {

        [Required]
        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("preferred_language")]
        public string PreferredLanguage { get; set; } = "en";

        [Required]
        [MaxLength(50)]
        [Column("preferred_channel")]
        public string PreferredChannel { get; set; } = "SMS"; // SMS, WhatsApp, Voice, InApp

        [Column("opt_out_sms")]
        public bool OptOutSms { get; set; } = false;

        [Column("opt_out_whats_app")]
        public bool OptOutWhatsApp { get; set; } = false;

        [Column("opt_out_voice")]
        public bool OptOutVoice { get; set; } = false;

        [Column("allow_appointment_reminders")]
        public bool AllowAppointmentReminders { get; set; } = true;

        [Column("allow_follow_up_reminders")]
        public bool AllowFollowUpReminders { get; set; } = true;

        [Column("allow_discharge_notifications")]
        public bool AllowDischargeNotifications { get; set; } = true;

        [Column("allow_lab_notifications")]
        public bool AllowLabNotifications { get; set; } = true;

        // Navigation properties
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;
    }
}
