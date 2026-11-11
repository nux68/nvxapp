using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Calendar.Models
{
    // feed iCalendar personale dell'utente nell'azienda corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-CAL.3)
    public class CalendarFeedModel
    {
        /// <summary>indirizzo https del feed (Google Calendar, Outlook)</summary>
        public string Url { get; set; } = string.Empty;
        /// <summary>stesso indirizzo con lo schema webcal:// (apre direttamente Apple Calendar / Outlook)</summary>
        public string WebcalUrl { get; set; } = string.Empty;
        public DateTime DataCreazione { get; set; }
        public DateTime? DataUltimoAccesso { get; set; }
    }

    public class CalendarFeedGetInModel
    {
        // crea il feed se l'utente non ne ha ancora uno (false = restituisce solo quello esistente)
        public bool Crea { get; set; } = true;
    }

    public class CalendarFeedGetOutModel : ModelResult
    {
        // null = nessun feed
        public CalendarFeedModel? Feed { get; set; }
    }

    public class CalendarFeedRegenerateInModel
    {
    }

    public class CalendarFeedRegenerateOutModel : ModelResult
    {
        public CalendarFeedModel? Feed { get; set; }
    }

    public class CalendarFeedDeleteInModel
    {
    }

    public class CalendarFeedDeleteOutModel : ModelResult
    {
    }
}
