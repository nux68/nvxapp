namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Calendar
{
    /*
     Eventi pubblicati dai moduli sul calendario personale dell'utente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-CAL.2).
     Ogni modulo aggiunge una classe che implementa ICalendarEventProvider: lo scanner dei servizi la registra e il feed
     iCalendar (CalendarFeedService) chiede gli eventi a tutte le implementazioni.
        - le implementazioni sono chiamate dal feed scaricato SENZA utente collegato (richiesta anonima con il token):
          non usano CurrentUserId / CurrentCompany ma i parametri; per le tabelle degli schemi delle aziende usano
          IApplicationDbContextFactory, come i deleter (CONTRIBUTING_nvxapp_data.md, REGOLA BASE punto 8)
        - restituiscono solo gli eventi che l'utente puo' vedere nell'azienda e solo se il loro applicativo e' attivo
        - gli Uid devono essere stabili (stesso oggetto = stesso Uid) e unici tra i moduli: prefisso del modulo
          (es. "scadenza-12", "crm.attivita-5")
    */
    public interface ICalendarEventProvider
    {
        /// <summary>eventi dell'utente nell'azienda con inizio tra dal e al (compresi)</summary>
        Task<IEnumerable<CalendarEvent>> EventsAsync(int idCompany, string idAspNetUsers, DateTime dal, DateTime al);
    }

    /// <summary>Evento del calendario (diventa un VEVENT del feed .ics).</summary>
    public class CalendarEvent
    {
        /// <summary>identificativo stabile e unico tra i moduli (es. "scadenza-12")</summary>
        public string Uid { get; set; } = string.Empty;

        public string Titolo { get; set; } = string.Empty;

        public string? Descrizione { get; set; }

        /// <summary>ora locale del server; per gli eventi tutto il giorno conta solo la data</summary>
        public DateTime Inizio { get; set; }

        /// <summary>fine (esclusa); null = un'ora dopo l'inizio, o il giorno stesso se tutto il giorno.
        /// Per gli eventi tutto il giorno e' l'ultimo giorno compreso</summary>
        public DateTime? Fine { get; set; }

        public bool TuttoIlGiorno { get; set; }

        /// <summary>pagina del client (relativa, es. "/deadlines": diventa assoluta con App:PublicUrl) o indirizzo assoluto</summary>
        public string? Url { get; set; }

        /// <summary>ultima modifica dell'oggetto (LAST-MODIFIED), se nota</summary>
        public DateTime? UltimaModifica { get; set; }
    }
}
