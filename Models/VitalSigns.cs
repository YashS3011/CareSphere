using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("vital_signs")]
    public class VitalSigns : BaseEntity
    {
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Column("bed_allotment_id")]
        public Guid BedAllotmentId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("recorded_by_user_id")]
        public string RecordedByUserId { get; set; } = string.Empty;

        [Column("temperature")]
        public decimal? Temperature { get; set; }         // Celsius

        [Column("blood_pressure_systolic")]
        public int? BloodPressureSystolic { get; set; }   // mmHg

        [Column("blood_pressure_diastolic")]
        public int? BloodPressureDiastolic { get; set; }  // mmHg

        [Column("heart_rate")]
        public int? HeartRate { get; set; }               // bpm

        [Column("spo2")]
        public int? SpO2 { get; set; }                    // percentage 0–100

        [Column("respiratory_rate")]
        public int? RespiratoryRate { get; set; }         // breaths per minute

        [Column("weight")]
        public decimal? Weight { get; set; }              // kg

        [Column("height")]
        public decimal? Height { get; set; }              // cm

        [Column("recorded_at")]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("BedAllotmentId")]
        public BedAllotment BedAllotment { get; set; } = null!;
    }
}
