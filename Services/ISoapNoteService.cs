using CareSphere.Models;

namespace CareSphere.Services
{
    public interface ISoapNoteService
    {
        Task<SoapNote> CreateSoapNoteAsync(SoapNote soapNote);
        Task<SoapNote?> GetSoapNoteByEncounterAsync(Guid encounterId);
        Task<SoapNote> UpdateSoapNoteAsync(SoapNote soapNote);
        Task FinalizeSoapNoteAsync(Guid soapNoteId);
    }
}
