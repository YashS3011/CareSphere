namespace CareSphere.Models.DTOs
{
    public class DoctorDashboardStats
    {
        public int WaitingCount { get; set; }
        public int InConsultationCount { get; set; }
        public int CompletedToday { get; set; }
        public int EncountersThisWeek { get; set; }
        public Doctor? Doctor { get; set; }
    }
}
