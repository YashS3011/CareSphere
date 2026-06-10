using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("doctors")]
    public class Doctor : BaseEntity
    {

        [Required]
        [MaxLength(100)]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("specialization")]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("registration_number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [MaxLength(30)]
        [Column("phone")]
        public string? Phone { get; set; }

        [MaxLength(150)]
        [EmailAddress]
        [Column("email")]
        public string? Email { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
