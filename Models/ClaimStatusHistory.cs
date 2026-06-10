using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("claim_status_history")]
    public class ClaimStatusHistory : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [Column("claim_id")]
        public Guid ClaimId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("previous_status")]
        public string PreviousStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("new_status")]
        public string NewStatus { get; set; } = string.Empty;

        [Required]
        [Column("changed_at")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        [Column("changed_by_user_id")]
        public string ChangedByUserId { get; set; } = "system";

        [Column("remarks")]
        public string? Remarks { get; set; }

        // Navigation properties
        [ForeignKey("ClaimId")]
        public InsuranceClaim InsuranceClaim { get; set; } = null!;
    }
}
