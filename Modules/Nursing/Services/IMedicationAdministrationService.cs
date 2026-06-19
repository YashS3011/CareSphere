using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Modules.Nursing.Services
{
    public interface IMedicationAdministrationService
    {
        Task<MedicationAdministrationRecord> RecordAdministrationAsync(
            MedicationAdministrationRecord mar, Guid tenantId);
        Task<List<MedicationAdministrationRecord>> GetByPatientAsync(
            Guid patientId, Guid tenantId);
        Task<List<MedicationAdministrationRecord>> GetByPrescriptionAsync(
            Guid prescriptionId, Guid tenantId);
    }
}
