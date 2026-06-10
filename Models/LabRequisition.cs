using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("lab_requisitions")]
    public class LabRequisition : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [MaxLength(100)]
        [Column("requisition_number")]
        public string RequisitionNumber { get; set; } = string.Empty;

        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Column("encounter_id")]
        public Guid? EncounterId { get; set; }

        [Column("ordered_by_doctor_id")]
        public Guid OrderedByDoctorId { get; set; }

        [Column("ordered_at")]
        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        [Column("priority")]
        public string Priority { get; set; } = "Routine"; // Routine, Urgent, Stat

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Ordered"; // Ordered, SampleCollected, Processing, Completed, Cancelled

        [Column("clinical_notes")]
        public string? ClinicalNotes { get; set; }

        [Column("fhir_service_request_json")]
        public string? FhirServiceRequestJson { get; set; }

        [MaxLength(100)]
        [Column("fhir_service_request_id")]
        public string? FhirServiceRequestId { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("EncounterId")]
        public Encounter? Encounter { get; set; }

        [ForeignKey("OrderedByDoctorId")]
        public Doctor OrderedByDoctor { get; set; } = null!;

        public ICollection<LabRequisitionItem> Items { get; set; } = new List<LabRequisitionItem>();
        public ICollection<LabSample> Samples { get; set; } = new List<LabSample>();
    }
}
