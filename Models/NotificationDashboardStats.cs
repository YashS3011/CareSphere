using System;

namespace CareSphere.Models
{
    public class NotificationDashboardStats
    {
        public int TotalSentToday { get; set; }
        public int TotalFailedToday { get; set; }
        public int TotalPendingReminders { get; set; }
        public int SmsSentToday { get; set; }
        public int WhatsAppSentToday { get; set; }
        public int VoiceSentToday { get; set; }
        public int InAppSentToday { get; set; }
        public double DeliveryRatePercent { get; set; }
    }
}
