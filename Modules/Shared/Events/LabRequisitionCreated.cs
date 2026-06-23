using System;
using System.Collections.Generic;

namespace CareSphere.Modules.Shared.Events
{
    public class LabRequisitionCreated
    {
        public Guid TenantId { get; set; }
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid RequisitionId { get; set; }
        public string RequisitionNumber { get; set; } = string.Empty;
        public List<LabTestLineItem> Tests { get; set; } = new();
    }

    public class LabTestLineItem
    {
        public Guid TestId { get; set; }
        public string TestCode { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public decimal Fee { get; set; }
    }
}
