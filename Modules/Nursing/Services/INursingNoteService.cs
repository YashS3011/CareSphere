using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Modules.Nursing.Services
{
    public interface INursingNoteService
    {
        Task<NursingNote> AddNoteAsync(NursingNote note, Guid tenantId);
        Task<List<NursingNote>> GetByPatientAsync(Guid patientId, Guid tenantId);
    }
}
