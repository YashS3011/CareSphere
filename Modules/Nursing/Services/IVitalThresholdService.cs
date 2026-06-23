using System.Collections.Generic;
using CareSphere.Models;

namespace CareSphere.Modules.Nursing.Services
{
    public interface IVitalThresholdService
    {
        List<string> CheckThresholds(VitalSigns vitals);
    }
}
