using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("lab_test_catalog")]
    public class LabTestCatalog : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Required]
        [MaxLength(100)]
        [Column("test_code")]
        public string TestCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("test_name")]
        public string TestName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("category")]
        public string Category { get; set; } = "Hematology"; // Hematology, Biochemistry, Microbiology, Immunology, Pathology, Radiology, Other

        [Required]
        [MaxLength(50)]
        [Column("sample_type")]
        public string SampleType { get; set; } = "Blood"; // Blood, Urine, Stool, Swab, Tissue, CSF, Other

        [Column("turnaround_hours")]
        public int TurnaroundHours { get; set; } = 24;

        [Column("price")]
        public decimal Price { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<LabTestParameter> Parameters { get; set; } = new List<LabTestParameter>();
    }
}
