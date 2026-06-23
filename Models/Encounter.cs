using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("encounters")]
    public class Encounter : BaseEntity
    {

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Required]
        [Column("doctor_id")]
        public Guid DoctorId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("encounter_type")]
        public string EncounterType { get; set; } = string.Empty; // OPD | IPD | Emergency

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Planned"; // Planned | InProgress | Completed

        [Required]
        [Column("admission_date")]
        public DateTime AdmissionDate { get; set; }

        [Column("discharge_date")]
        public DateTime? DischargeDate { get; set; }

        [Column("discharge_disposition")]
        [MaxLength(50)]
        public string? DischargeDisposition { get; set; }

        [Column("chief_complaint")]
        public string? ChiefComplaint { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        // Navigation properties
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; } = null!;

        public ICollection<SoapNote> SoapNotes { get; set; } = new List<SoapNote>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<TeleConsultSession> TeleConsultSessions { get; set; } = new List<TeleConsultSession>();
    }
}
