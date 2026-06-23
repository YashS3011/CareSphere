using System.Collections.Generic;
using CareSphere.Models;

namespace CareSphere.Modules.Nursing.Services
{
    public class VitalThresholdService : IVitalThresholdService
    {
        public List<string> CheckThresholds(VitalSigns vitals)
        {
            var breaches = new List<string>();

            if (vitals.SpO2.HasValue && vitals.SpO2.Value < 92)
            {
                breaches.Add($"SpO2 ({vitals.SpO2.Value}%)");
            }

            if (vitals.HeartRate.HasValue && (vitals.HeartRate.Value > 130 || vitals.HeartRate.Value < 40))
            {
                breaches.Add($"Heart Rate ({vitals.HeartRate.Value} bpm)");
            }

            if (vitals.BloodPressureSystolic.HasValue && (vitals.BloodPressureSystolic.Value > 180 || vitals.BloodPressureSystolic.Value < 80))
            {
                breaches.Add($"Systolic BP ({vitals.BloodPressureSystolic.Value} mmHg)");
            }

            if (vitals.Temperature.HasValue && (vitals.Temperature.Value > 39.5m || vitals.Temperature.Value < 35.0m))
            {
                breaches.Add($"Temperature ({vitals.Temperature.Value}°C)");
            }

            return breaches;
        }
    }
}
