using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("medication_administration_records")]
    public class MedicationAdministrationRecord : BaseEntity
    {
        [Column("prescription_id")]
        public Guid PrescriptionId { get; set; }

        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nurse_user_id")]
        public string NurseUserId { get; set; } = string.Empty;

        [Column("administered_at")]
        public DateTime AdministeredAt { get; set; } = DateTime.UtcNow;

        [Column("dose_given")]
        public decimal DoseGiven { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("dose_unit")]
        public string DoseUnit { get; set; } = string.Empty;       // "mg", "ml", "units", "tablet"

        [Required]
        [MaxLength(50)]
        [Column("route")]
        public string Route { get; set; }          // "Oral", "IV", "IM", "SC", "Topical"

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; }         // "Administered", "Skipped", "Refused"

        [Column("skip_reason")]
        public string? SkipReason { get; set; }     // populated if Status = Skipped/Refused

        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation
        [ForeignKey("PrescriptionId")]
        public Prescription Prescription { get; set; } = null!;

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;
    }
}
