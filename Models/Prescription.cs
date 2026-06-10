using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("prescriptions")]
    public class Prescription : BaseEntity
    {

        [Required]
        [Column("encounter_id")]
        public Guid EncounterId { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Required]
        [Column("doctor_id")]
        public Guid DoctorId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("drug_name")]
        public string DrugName { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("drug_code")]
        public string? DrugCode { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("form")]
        public string Form { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("strength")]
        public string Strength { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("frequency")]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("duration")]
        public string Duration { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("route")]
        public string Route { get; set; } = string.Empty;

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Active"; // Active | Cancelled

        [Column("issued_at")]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [Column("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        [Column("cancellation_reason")]
        public string? CancellationReason { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        // Navigation properties
        [ForeignKey("EncounterId")]
        public Encounter Encounter { get; set; } = null!;

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; } = null!;
    }
}
