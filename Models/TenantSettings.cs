using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    /// <summary>
    /// Hospital/tenant configuration settings including SSO, subscription, and branding.
    /// One row per tenant (TenantId is unique).
    /// </summary>
    [Table("tenant_settings")]
    public class TenantSettings : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("hospital_name")]
        public string HospitalName { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("hospital_logo_path")]
        public string? HospitalLogoPath { get; set; }

        [MaxLength(300)]
        [Column("address_line1")]
        public string? AddressLine1 { get; set; }

        [MaxLength(300)]
        [Column("address_line2")]
        public string? AddressLine2 { get; set; }

        [MaxLength(100)]
        [Column("city")]
        public string? City { get; set; }

        [MaxLength(100)]
        [Column("state")]
        public string? State { get; set; }

        [MaxLength(20)]
        [Column("pin_code")]
        public string? PinCode { get; set; }

        [MaxLength(30)]
        [Column("phone")]
        public string? Phone { get; set; }

        [MaxLength(200)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(100)]
        [Column("nabh_registration_number")]
        public string? NabhRegistrationNumber { get; set; }

        [MaxLength(100)]
        [Column("abdm_facility_id")]
        public string? AbdmFacilityId { get; set; }

        // --- SSO Configuration ---

        [Column("sso_enabled")]
        public bool SsoEnabled { get; set; } = false;

        /// <summary>Allowed values: Google, Microsoft, SAML, OIDC</summary>
        [MaxLength(50)]
        [Column("sso_provider")]
        public string? SsoProvider { get; set; }

        [MaxLength(500)]
        [Column("oidc_client_id")]
        public string? OidcClientId { get; set; }

        [MaxLength(500)]
        [Column("oidc_client_secret")]
        public string? OidcClientSecret { get; set; }

        [MaxLength(500)]
        [Column("oidc_authority")]
        public string? OidcAuthority { get; set; }

        [MaxLength(1000)]
        [Column("saml_metadata_url")]
        public string? SamlMetadataUrl { get; set; }

        // --- Subscription ---

        [Column("max_users_allowed")]
        public int MaxUsersAllowed { get; set; } = 100;

        /// <summary>Allowed values: Free, Basic, Pro, Enterprise</summary>
        [MaxLength(20)]
        [Column("subscription_tier")]
        public string SubscriptionTier { get; set; } = "Free";

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
