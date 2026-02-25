using CareSphere.Data;
using CareSphere.Models;
using CareSphere.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class EncounterService : IEncounterService
    {
        private readonly ApplicationDbContext _context;

        public EncounterService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Queue ---
        public async Task<List<OpdQueue>> GetQueueByDoctorAndDateAsync(Guid doctorId, DateOnly date)
        {
            return await _context.OpdQueues
                .Include(q => q.Patient)
                .Where(q => q.DoctorId == doctorId && q.QueueDate == date)
                .OrderBy(q => q.TokenNumber)
                .ToListAsync();
        }

        public async Task<OpdQueue> AddToQueueAsync(OpdQueue entry)
        {
            _context.OpdQueues.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task UpdateQueueStatusAsync(Guid queueId, string status)
        {
            var queueEntry = await _context.OpdQueues.FindAsync(queueId);
            if (queueEntry != null)
            {
                // A queue entry can only be moved to InConsultation if status is currently Waiting
                if (status == "InConsultation" && queueEntry.Status != "Waiting")
                {
                    throw new InvalidOperationException("Can only move 'Waiting' patients to consultation.");
                }

                queueEntry.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetNextTokenNumberAsync(Guid doctorId, DateOnly date)
        {
            // Token number resets to 1 for each doctor each day
            var lastEntry = await _context.OpdQueues
                .Where(q => q.DoctorId == doctorId && q.QueueDate == date)
                .OrderByDescending(q => q.TokenNumber)
                .FirstOrDefaultAsync();

            return lastEntry == null ? 1 : lastEntry.TokenNumber + 1;
        }

        // --- Encounter ---
        public async Task<Encounter> StartEncounterAsync(Encounter encounter)
        {
            // Only one active encounter per patient at a time (status = InProgress)
            var activeEncounter = await _context.Encounters
                .FirstOrDefaultAsync(e => e.PatientId == encounter.PatientId && e.Status == "InProgress");

            if (activeEncounter != null)
            {
                throw new InvalidOperationException("This patient already has an active encounter in progress.");
            }

            // Also validate status flow: Planned -> InProgress -> Finished
            if (encounter.Status != "Planned" && encounter.Status != "InProgress")
            {
                throw new InvalidOperationException("New encounter must start as 'Planned' or 'InProgress'.");
            }

            _context.Encounters.Add(encounter);
            await _context.SaveChangesAsync();
            return encounter;
        }

        public async Task<Encounter?> GetEncounterByIdAsync(Guid id)
        {
            return await _context.Encounters
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .Include(e => e.Queue)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Encounter>> GetEncountersByPatientAsync(Guid patientId)
        {
            return await _context.Encounters
                .Include(e => e.Doctor)
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.StartDatetime)
                .ToListAsync();
        }

        public async Task UpdateEncounterStatusAsync(Guid encounterId, string status)
        {
            var encounter = await _context.Encounters.FindAsync(encounterId);
            if (encounter != null)
            {
                // Validate status flow
                var validTransitions = new Dictionary<string, List<string>>
                {
                    { "Planned", new List<string> { "InProgress", "Cancelled" } },
                    { "InProgress", new List<string> { "Finished" } },
                    { "Finished", new List<string>() },
                    { "Cancelled", new List<string>() }
                };

                if (!validTransitions.ContainsKey(encounter.Status) || !validTransitions[encounter.Status].Contains(status))
                {
                     throw new InvalidOperationException($"Invalid encounter state transition from {encounter.Status} to {status}.");
                }

                encounter.Status = status;

                if (status == "Finished")
                {
                    encounter.EndDatetime = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task FinishEncounterAsync(Guid encounterId)
        {
            await UpdateEncounterStatusAsync(encounterId, "Finished");
        }

        // --- SOAP Note ---
        public async Task SaveSoapNoteAsync(SoapNote note)
        {
            // One-per-encounter
            var existingNote = await _context.SoapNotes
                .FirstOrDefaultAsync(s => s.EncounterId == note.EncounterId);

            if (existingNote != null)
            {
                // Update
                existingNote.Subjective = note.Subjective;
                existingNote.ObjectiveTemp = note.ObjectiveTemp;
                existingNote.ObjectiveBp = note.ObjectiveBp;
                existingNote.ObjectivePulse = note.ObjectivePulse;
                existingNote.ObjectiveSpo2 = note.ObjectiveSpo2;
                existingNote.ObjectiveRr = note.ObjectiveRr;
                existingNote.ObjectiveWeight = note.ObjectiveWeight;
                existingNote.ObjectiveHeight = note.ObjectiveHeight;
                existingNote.ObjectiveBmi = note.ObjectiveBmi;
                existingNote.ObjectiveNotes = note.ObjectiveNotes;
                existingNote.Assessment = note.Assessment;
                existingNote.Plan = note.Plan;
                existingNote.UpdatedAt = DateTime.UtcNow;

                _context.SoapNotes.Update(existingNote);
            }
            else
            {
                // Create
                _context.SoapNotes.Add(note);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<SoapNote?> GetSoapNoteByEncounterAsync(Guid encounterId)
        {
            return await _context.SoapNotes.FirstOrDefaultAsync(s => s.EncounterId == encounterId);
        }

        // --- Diagnosis ---
        public async Task AddDiagnosisAsync(Diagnosis diagnosis)
        {
            _context.Diagnoses.Add(diagnosis);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDiagnosisAsync(Diagnosis diagnosis)
        {
            _context.Diagnoses.Update(diagnosis);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDiagnosisAsync(Guid id)
        {
             var diagnosis = await _context.Diagnoses.FindAsync(id);
             if(diagnosis != null)
             {
                 _context.Diagnoses.Remove(diagnosis);
                 await _context.SaveChangesAsync();
             }
        }

        public async Task<List<Diagnosis>> GetDiagnosesByEncounterAsync(Guid encounterId)
        {
            return await _context.Diagnoses
                .Where(d => d.EncounterId == encounterId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        // --- Prescription ---
        public async Task AddPrescriptionAsync(Prescription prescription)
        {
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePrescriptionAsync(Prescription prescription)
        {
            _context.Prescriptions.Update(prescription);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePrescriptionAsync(Guid id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Prescription>> GetPrescriptionsByEncounterAsync(Guid encounterId)
        {
            return await _context.Prescriptions
                .Where(p => p.EncounterId == encounterId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CheckAllergyWarningAsync(Guid patientId, string medicineName)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null || string.IsNullOrWhiteSpace(patient.AllergyNotes) || string.IsNullOrWhiteSpace(medicineName))
            {
                return false;
            }

            // Simple partial match check
            return patient.AllergyNotes.Contains(medicineName, StringComparison.OrdinalIgnoreCase);
        }

        // --- Procedure ---
        public async Task AddProcedureAsync(Procedure procedure)
        {
            _context.Procedures.Add(procedure);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Procedure>> GetProceduresByEncounterAsync(Guid encounterId)
        {
            return await _context.Procedures
                .Include(p => p.PerformedByDoctor)
                .Where(p => p.EncounterId == encounterId)
                .OrderByDescending(p => p.PerformedDatetime)
                .ToListAsync();
        }

        // --- Discharge Summary ---
        public async Task SaveDischargeSummaryAsync(DischargeSummary summary)
        {
            // Discharge summary only available for encounters where encounter_type = IPD
            var encounter = await _context.Encounters.FindAsync(summary.EncounterId);
            if (encounter == null || encounter.EncounterType != "IPD")
            {
                throw new InvalidOperationException("Discharge summary can only be created for IPD encounters.");
            }

            var existingSummary = await _context.DischargeSummaries
                .FirstOrDefaultAsync(d => d.EncounterId == summary.EncounterId);

            if (existingSummary != null)
            {
                // Update
                existingSummary.AdmissionDate = summary.AdmissionDate;
                existingSummary.DischargeDate = summary.DischargeDate;
                existingSummary.FinalDiagnosis = summary.FinalDiagnosis;
                existingSummary.TreatmentSummary = summary.TreatmentSummary;
                existingSummary.ConditionAtDischarge = summary.ConditionAtDischarge;
                existingSummary.FollowupInstructions = summary.FollowupInstructions;
                existingSummary.AuthorizedByDoctorId = summary.AuthorizedByDoctorId;

                _context.DischargeSummaries.Update(existingSummary);
            }
            else
            {
                _context.DischargeSummaries.Add(summary);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<DischargeSummary?> GetDischargeSummaryByEncounterAsync(Guid encounterId)
        {
             return await _context.DischargeSummaries
                .Include(d => d.AuthorizedByDoctor)
                .FirstOrDefaultAsync(d => d.EncounterId == encounterId);
        }

        // --- Dashboard ---
        public async Task<DoctorDashboardStats> GetDoctorDashboardAsync(Guid doctorId, DateOnly date)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);

            var queueToday = await _context.OpdQueues
                .Where(q => q.DoctorId == doctorId && q.QueueDate == date)
                .ToListAsync();

            var waitingCount = queueToday.Count(q => q.Status == "Waiting");
            var inConsultationCount = queueToday.Count(q => q.Status == "InConsultation");
            var completedToday = queueToday.Count(q => q.Status == "Completed");

            // Assuming week starts on Sunday
            var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
            var dateTimeStartOfWeek = startOfWeek.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var dateTimeEndOfWeek = startOfWeek.AddDays(7).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            var encountersThisWeek = await _context.Encounters
                .Where(e => e.DoctorId == doctorId &&
                            e.StartDatetime >= dateTimeStartOfWeek &&
                            e.StartDatetime < dateTimeEndOfWeek)
                .CountAsync();

            return new DoctorDashboardStats
            {
                Doctor = doctor,
                WaitingCount = waitingCount,
                InConsultationCount = inConsultationCount,
                CompletedToday = completedToday,
                EncountersThisWeek = encountersThisWeek
            };
        }
    }
}
