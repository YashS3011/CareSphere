using System;

namespace CareSphere.Modules.Shared.Events
{
    public class AppointmentBookedEvent
    {
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime SlotStart { get; set; }
        public Guid TenantId { get; set; }
    }
}
