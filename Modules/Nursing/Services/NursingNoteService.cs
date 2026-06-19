using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;

namespace CareSphere.Modules.Nursing.Services
{
    public class NursingNoteService : INursingNoteService
    {
        private readonly ApplicationDbContext _dbContext;

        public NursingNoteService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NursingNote> AddNoteAsync(NursingNote note, Guid tenantId)
        {
            note.Id = Guid.NewGuid();
            note.TenantId = tenantId;
            if (note.NoteDateTime == default)
            {
                note.NoteDateTime = DateTime.UtcNow;
            }
            note.CreatedAt = DateTime.UtcNow;

            _dbContext.NursingNotes.Add(note);
            await _dbContext.SaveChangesAsync();
            return note;
        }

        public async Task<List<NursingNote>> GetByPatientAsync(Guid patientId, Guid tenantId)
        {
            return await _dbContext.NursingNotes
                .Where(n => n.PatientId == patientId && n.TenantId == tenantId)
                .OrderByDescending(n => n.NoteDateTime)
                .ToListAsync();
        }
    }
}
