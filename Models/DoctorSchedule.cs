using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSphere.Models
{
    [Table("doctor_schedules")]
    public class DoctorSchedule : BaseEntity
    {
        [Column("doctor_id")]
        public Guid DoctorId { get; set; }

        [Column("tenant_id")]
        public Guid TenantId { get; set; }

        [Column("day_of_week")]
        public int DayOfWeek { get; set; }        // 0=Sunday … 6=Saturday

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Column("slot_duration_minutes")]
        public int SlotDurationMinutes { get; set; } // e.g. 15, 20, 30

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation
        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; } = null!;
    }
}
