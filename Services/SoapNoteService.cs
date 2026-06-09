using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class SoapNoteService : ISoapNoteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public SoapNoteService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<SoapNote> CreateSoapNoteAsync(SoapNote soapNote)
        {
            soapNote.Id = Guid.NewGuid();
            soapNote.CreatedAt = DateTime.UtcNow;
            soapNote.Status = "Draft";

            _context.SoapNotes.Add(soapNote);
            await _context.SaveChangesAsync();
            return soapNote;
        }

        public async Task<SoapNote?> GetSoapNoteByEncounterAsync(Guid encounterId)
        {
            return await _context.SoapNotes
                .Include(s => s.CreatedByDoctor)
                .FirstOrDefaultAsync(s => s.EncounterId == encounterId);
        }

        public async Task<SoapNote> UpdateSoapNoteAsync(SoapNote soapNote)
        {
            var existing = await _context.SoapNotes.FindAsync(soapNote.Id);
            if (existing == null)
                throw new InvalidOperationException("SOAP note not found.");

            if (existing.Status == "Final")
                throw new InvalidOperationException("Cannot edit a finalized SOAP note.");

            existing.Subjective = soapNote.Subjective;
            existing.Objective = soapNote.Objective;
            existing.Assessment = soapNote.Assessment;
            existing.Plan = soapNote.Plan;

            _context.SoapNotes.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task FinalizeSoapNoteAsync(Guid soapNoteId)
        {
            var soapNote = await _context.SoapNotes.FindAsync(soapNoteId);
            if (soapNote == null)
                throw new InvalidOperationException("SOAP note not found.");

            if (soapNote.Status == "Final")
                throw new InvalidOperationException("SOAP note is already finalized.");

            soapNote.Status = "Final";
            soapNote.FinalizedAt = DateTime.UtcNow;

            _context.SoapNotes.Update(soapNote);
            await _context.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system", // Will be replaced once auth is added
                Action = "SOAP_NOTE_FINALIZED",
                ResourceType = "SoapNote",
                ResourceId = soapNoteId.ToString(),
                TenantId = soapNote.TenantId
            });
        }
    }
}
