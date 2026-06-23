using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("shift_handovers")]
    public class ShiftHandover : BaseEntity
    {
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("outgoing_nurse_id")]
        public string OutgoingNurseId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("incoming_nurse_id")]
        public string IncomingNurseId { get; set; } = string.Empty;

        [Required]
        [Column("handover_notes")]
        public string HandoverNotes { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("shift")]
        public string Shift { get; set; } = string.Empty; // Morning | Evening | Night

        [Column("handover_date")]
        public DateTime HandoverDate { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;
    }
}
