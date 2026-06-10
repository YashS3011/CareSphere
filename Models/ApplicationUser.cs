using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    /// <summary>
    /// Extended Identity user with CareSphere-specific properties.
    /// Inherits ASP.NET Core Identity's IdentityUser base class.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>Multi-tenant isolation key — every user belongs to exactly one tenant.</summary>
        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        /// <summary>Display name of the user.</summary>
        [Required]
        [MaxLength(200)]
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>Primary role string (mirrors Identity role). Kept denormalised for quick reads.</summary>
        [MaxLength(50)]
        [Column("role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>Department the user belongs to (optional).</summary>
        [MaxLength(100)]
        [Column("department")]
        public string? Department { get; set; }

        /// <summary>Whether the account is active. Deactivated users cannot log in.</summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>UTC timestamp of the most recent successful login.</summary>
        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        /// <summary>Preferred UI language (BCP-47 code, e.g. "en", "hi").</summary>
        [MaxLength(10)]
        [Column("preferred_language")]
        public string PreferredLanguage { get; set; } = "en";

        /// <summary>Relative path to profile image stored in wwwroot.</summary>
        [MaxLength(500)]
        [Column("profile_image_path")]
        public string? ProfileImagePath { get; set; }

        /// <summary>Record creation timestamp.</summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Optional link to a Doctor record — set when Role is Doctor so that
        /// doctor-specific workflows can use the linked record directly.
        /// </summary>
        [Column("doctor_id")]
        public Guid? DoctorId { get; set; }
    }
}
