using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Calendar;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Calendar.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Feed iCalendar personale (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-CAL.3, decisione D9)
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarFeedController : NvxControllerBase
    {
        private readonly ICalendarFeedService _service;

        public CalendarFeedController(IHttpContextAccessor httpContextAccessor,
                                      ICalendarFeedService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        /// <summary>
        /// Feed .ics sottoscritto dai calendari (Google, Outlook, Apple): ANONIMO, il token nell'indirizzo e' l'unica protezione.
        /// GET api/CalendarFeed/{token}.ics ; token sconosciuto = 404
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        [Route("{token}.ics")]
        public async Task<IActionResult> Ics(string token)
        {
            var ics = await _service.IcsAsync(token);
            if (ics == null)
                return NotFound();

            // i calendari rileggono il feed periodicamente: niente cache intermedie
            Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            Response.Headers["X-Robots-Tag"] = "noindex";
            return Content(ics, "text/calendar; charset=utf-8");
        }

        [Authorize]
        [HttpPost]
        [Route("CalendarFeedGet")]
        public async Task<GenericResult<CalendarFeedGetOutModel>> CalendarFeedGet(GenericRequest<CalendarFeedGetInModel> inModel)
        {
            return await _service.CalendarFeedGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("CalendarFeedRegenerate")]
        public async Task<GenericResult<CalendarFeedRegenerateOutModel>> CalendarFeedRegenerate(GenericRequest<CalendarFeedRegenerateInModel> inModel)
        {
            return await _service.CalendarFeedRegenerate(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("CalendarFeedDelete")]
        public async Task<GenericResult<CalendarFeedDeleteOutModel>> CalendarFeedDelete(GenericRequest<CalendarFeedDeleteInModel> inModel)
        {
            return await _service.CalendarFeedDelete(inModel, false);
        }
    }
}
