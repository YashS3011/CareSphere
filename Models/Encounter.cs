using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("encounters")]
    public class Encounter
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        [Column("doctor_id")]
        public Guid DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }

        [Column("queue_id")]
        public Guid? QueueId { get; set; }

        public OpdQueue? Queue { get; set; }

        [Column("bed_allotment_id")]
        public Guid? BedAllotmentId { get; set; }

        [ForeignKey("BedAllotmentId")]
        public BedAllotment? BedAllotment { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("encounter_type")]
        public string EncounterType { get; set; } = string.Empty; // OPD | IPD | Emergency | Tele

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Planned"; // Planned | InProgress | Finished | Cancelled

        [Required]
        [Column("chief_complaint")]
        public string ChiefComplaint { get; set; } = string.Empty;

        [Column("visit_notes")]
        public string? VisitNotes { get; set; }

        [Required]
        [Column("start_datetime")]
        public DateTime StartDatetime { get; set; } = DateTime.UtcNow;

        [Column("end_datetime")]
        public DateTime? EndDatetime { get; set; }

        [MaxLength(100)]
        [Column("fhir_encounter_id")]
        public string? FhirEncounterId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
