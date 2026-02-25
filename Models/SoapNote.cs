using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("soap_notes")]
    public class SoapNote
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
        [Column("subjective")]
        public string Subjective { get; set; } = string.Empty;

        [Column("objective_temp")]
        public decimal? ObjectiveTemp { get; set; }

        [MaxLength(20)]
        [Column("objective_bp")]
        public string? ObjectiveBp { get; set; }

        [Column("objective_pulse")]
        public int? ObjectivePulse { get; set; }

        [Column("objective_spo2")]
        public decimal? ObjectiveSpo2 { get; set; }

        [Column("objective_rr")]
        public int? ObjectiveRr { get; set; }

        [Column("objective_weight")]
        public decimal? ObjectiveWeight { get; set; }

        [Column("objective_height")]
        public decimal? ObjectiveHeight { get; set; }

        [Column("objective_bmi")]
        public decimal? ObjectiveBmi { get; set; }

        [Column("objective_notes")]
        public string? ObjectiveNotes { get; set; }

        [Required]
        [Column("assessment")]
        public string Assessment { get; set; } = string.Empty;

        [Required]
        [Column("plan")]
        public string Plan { get; set; } = string.Empty;

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
