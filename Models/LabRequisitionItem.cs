using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("lab_requisition_items")]
    public class LabRequisitionItem : BaseEntity
    {

        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.Empty;

        [Column("requisition_id")]
        public Guid RequisitionId { get; set; }

        [Column("test_id")]
        public Guid TestId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Ordered"; // Ordered, SampleCollected, Processing, Completed, Cancelled

        [Column("billing_line_item_id")]
        public Guid? BillingLineItemId { get; set; }

        // Navigation properties
        [ForeignKey("RequisitionId")]
        public LabRequisition Requisition { get; set; } = null!;

        [ForeignKey("TestId")]
        public LabTestCatalog Test { get; set; } = null!;

        public ICollection<LabResult> Results { get; set; } = new List<LabResult>();
    }
}
