using System;

namespace CareSphere.Modules.Shared.Events
{
    public class PatientDischarged
    {
        public Guid TenantId { get; set; }
        public Guid PatientId { get; set; }
        public Guid AllotmentId { get; set; }
        public DateTime DischargeDate { get; set; }
        public string Language { get; set; } = "en";
    }
}
