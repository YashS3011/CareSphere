using System;

namespace CareSphere.Modules.Shared.Events
{
    public class PatientAdmitted
    {
        public Guid TenantId { get; set; }
        public Guid PatientId { get; set; }
        public Guid AllotmentId { get; set; }
        public Guid BedId { get; set; }
        public string WardName { get; set; } = string.Empty;
        public string BedNumber { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public string AdmissionType { get; set; } = "IPD"; // OPD | IPD | Emergency
    }
}
