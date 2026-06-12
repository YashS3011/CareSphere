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

namespace CareSphere.Modules.Admin.Services
{
    public class UserTimeZoneService
    {
        public int? TimeZoneOffset { get; set; }

        public DateTime ToLocal(DateTime utcDateTime)
        {
            if (!TimeZoneOffset.HasValue)
                return utcDateTime; // Fallback to UTC/raw

            // ensure utcDateTime has Utc kind
            var utc = utcDateTime.Kind == DateTimeKind.Utc 
                ? utcDateTime 
                : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            
            // getTimezoneOffset() returns minutes browser is behind UTC.
            // e.g. IST (UTC+5:30) is -330 minutes, so AddMinutes(-(-330)) = AddMinutes(330).
            return utc.AddMinutes(-TimeZoneOffset.Value);
        }

        public DateTime? ToLocal(DateTime? utcDateTime)
        {
            if (utcDateTime == null) return null;
            return ToLocal(utcDateTime.Value);
        }
    }
}
