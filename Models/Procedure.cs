using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("procedures")]
    public class Procedure
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
        [Column("procedure_name")]
        public string ProcedureName { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("procedure_code")]
        public string? ProcedureCode { get; set; }

        [Required]
        [Column("performed_datetime")]
        public DateTime PerformedDatetime { get; set; }

        [Required]
        [Column("performed_by_doctor_id")]
        public Guid PerformedByDoctorId { get; set; }

        [ForeignKey("PerformedByDoctorId")]
        public Doctor? PerformedByDoctor { get; set; }

        [Column("outcome")]
        public string? Outcome { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [MaxLength(100)]
        [Column("fhir_procedure_id")]
        public string? FhirProcedureId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
