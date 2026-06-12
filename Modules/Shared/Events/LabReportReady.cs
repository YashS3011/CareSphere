using CareSphere.Modules.Clinical.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Patients.Services;
using CareSphere.Modules.Ward.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Shared.Services;
using CareSphere.Modules.Shared.Events;
using System;

namespace CareSphere.Modules.Shared.Events
{
    public class LabReportReady
    {
        public Guid LabReportId { get; set; }
        public Guid PatientId { get; set; }
        public Guid TenantId { get; set; }
    }
}
