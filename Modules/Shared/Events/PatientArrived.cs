using System;

namespace CareSphere.Modules.Shared.Events
{
    public class PatientArrived
    {
        public Guid TenantId { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid AppointmentId { get; set; }
    }
}
