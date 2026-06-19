using System;
using System.Collections.Generic;

namespace CareSphere.Modules.Analytics.Models
{
    public class BedOccupancyMetrics
    {
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int AvailableBeds { get; set; }
        public decimal OccupancyRatePercent { get; set; }
        public double AverageLengthOfStayDays { get; set; }
    }

    public class RevenueMetrics
    {
        public decimal TotalInvoiced { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class LabMetrics
    {
        public int TotalRequisitionsThisMonth { get; set; }
        public int AbnormalResultsThisMonth { get; set; }
        public double AverageTurnaroundHours { get; set; }
    }

    public class TopDoctorsMetrics
    {
        public List<DoctorEncounterCount> TopDoctors { get; set; } = new();
    }

    public class DoctorEncounterCount
    {
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public int EncounterCount { get; set; }
    }

    public class PharmacyMetrics
    {
        public decimal TotalStockValue { get; set; }
        public int LowStockItemsCount { get; set; }
        public int NearExpiryBatchesCount { get; set; }
    }
}
