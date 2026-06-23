using System;

namespace CareSphere.Modules.Shared.Events
{
    public class CriticalResultFlagged
    {
        public Guid TenantId { get; set; }
        public Guid PatientId { get; set; }
        public Guid RequisitionId { get; set; }
        public Guid ResultId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string ResultValue { get; set; } = string.Empty;
        public string AbnormalFlag { get; set; } = string.Empty; // HH or LL
        public Guid OrderingDoctorId { get; set; }
        public string OrderingDoctorUserId { get; set; } = string.Empty;
    }
}
