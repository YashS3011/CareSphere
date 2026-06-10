using System;

namespace CareSphere.Services
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
