using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("nursing_notes")]
    public class NursingNote : BaseEntity
    {
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Column("bed_allotment_id")]
        public Guid BedAllotmentId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nurse_user_id")]
        public string NurseUserId { get; set; } = string.Empty;

        [Required]
        [Column("note")]
        public string Note { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("note_type")]
        public string NoteType { get; set; } = string.Empty;  // "General", "PreOp", "PostOp", "Handover", "Incident"

        [Column("note_date_time")]
        public DateTime NoteDateTime { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("BedAllotmentId")]
        public BedAllotment BedAllotment { get; set; } = null!;
    }
}
