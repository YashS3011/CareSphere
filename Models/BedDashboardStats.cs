namespace CareSphere.Models
{
    public class BedDashboardStats
    {
        public int TotalBeds { get; set; }
        public int Available { get; set; }
        public int Occupied { get; set; }
        public int Maintenance { get; set; }
        public int Reserved { get; set; }
        public List<WardSummary> WardBreakdown { get; set; } = new List<WardSummary>();
    }

    public class WardSummary
    {
        public string WardName { get; set; } = string.Empty;
        public string WardType { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Occupied { get; set; }
        public int Available { get; set; }
    }
}
