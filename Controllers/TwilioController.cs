using System;
using System.Threading.Tasks;
using CareSphere.Data;
using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace CareSphere.Controllers
{
    [ApiController]
    [Route("api/twilio/twiml")]
    public class TwilioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TwilioController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region FR-038 RECOMMENDED FEATURE BEGIN
        // this endpoint must be publicly accessible for Twilio to reach it in production
        [HttpGet("{notificationLogId}")]
        public async Task<IActionResult> GetTwiml(Guid notificationLogId)
        {
            var log = await _context.NotificationLogs.FindAsync(notificationLogId);
            if (log == null || !log.Channel.Equals("Voice", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound("Log not found or not a voice channel notification.");
            }

            var twilioLanguage = log.Language switch
            {
                "en" => "en-IN",
                "hi" => "hi-IN",
                "ta" => "ta-IN",
                "te" => "te-IN",
                _ => "en-IN"
            };

            var response = new VoiceResponse();
            var say = new Say(log.MessageBody, voice: Say.VoiceEnum.Alice, language: twilioLanguage);
            response.Append(say);

            return Content(response.ToString(), "application/xml");
        }
        #endregion FR-038 RECOMMENDED FEATURE END
    }
}
