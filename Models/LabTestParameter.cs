using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("lab_test_parameters")]
    public class LabTestParameter : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("test_id")]
        public Guid TestId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("parameter_name")]
        public string ParameterName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("parameter_code")]
        public string? ParameterCode { get; set; }

        [MaxLength(50)]
        [Column("unit")]
        public string? Unit { get; set; }

        [Column("reference_range_low")]
        public decimal? ReferenceRangeLow { get; set; }

        [Column("reference_range_high")]
        public decimal? ReferenceRangeHigh { get; set; }

        [Column("reference_range_text")]
        public string? ReferenceRangeText { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("data_type")]
        public string DataType { get; set; } = "Numeric"; // Numeric, Text, Boolean

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        // Navigation property
        [ForeignKey("TestId")]
        public LabTestCatalog Test { get; set; } = null!;
    }
}
