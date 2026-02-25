using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("patients")]
    public class Patient
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("mrn")]
        public string Mrn { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First name can only contain alphabetic characters.")]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Last name can only contain alphabetic characters.")]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Column("date_of_birth", TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("gender")]
        public string Gender { get; set; } = string.Empty; // Male / Female / Other

        [Required]
        [MaxLength(30)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits and only numeric.")]
        [Column("phone")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(150)]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [Column("email")]
        public string? Email { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9\s,.\-]*$", ErrorMessage = "Address must be alphanumeric.")]
        [Column("address")]
        public string? Address { get; set; }

        [MaxLength(50)]
        [Column("abha_id")]
        public string? AbhaId { get; set; }

        [MaxLength(10)]
        [Column("blood_group")]
        public string? BloodGroup { get; set; }

        [Column("allergy_notes")]
        public string? AllergyNotes { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
