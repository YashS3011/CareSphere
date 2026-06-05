using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("bed_allotments")]
    public class BedAllotment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("bed_id")]
        public Guid BedId { get; set; }

        [Required]
        [Column("patient_id")]
        public Guid PatientId { get; set; }

        [Required]
        [Column("admission_date")]
        public DateTime AdmissionDate { get; set; }

        [Column("discharge_date")]
        public DateTime? DischargeDate { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("admission_type")]
        public string AdmissionType { get; set; } = string.Empty; // OPD | IPD | Emergency

        [MaxLength(100)]
        [Column("admitting_doctor")]
        public string? AdmittingDoctor { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("discharge_notes")]
        public string? DischargeNotes { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Active"; // Active | Discharged | Transferred

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BedId")]
        public Bed Bed { get; set; } = null!;

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        public ICollection<BedTransfer> Transfers { get; set; } = new List<BedTransfer>();
    }
}
