using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("prescriptions")]
    public class Prescription
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("encounter_id")]
        public Guid EncounterId { get; set; }

        [ForeignKey("EncounterId")]
        public Encounter? Encounter { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("medicine_name")]
        public string MedicineName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("dosage")]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("route")]
        public string Route { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("frequency")]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("duration")]
        public string Duration { get; set; } = string.Empty;

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("instructions")]
        public string? Instructions { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("continue_after_discharge")]
        public bool ContinueAfterDischarge { get; set; } = false;

        [MaxLength(100)]
        [Column("fhir_medication_request_id")]
        public string? FhirMedicationRequestId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
