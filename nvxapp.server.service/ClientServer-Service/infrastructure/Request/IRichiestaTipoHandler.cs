using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Request
{
    /*
     Richieste da approvare comuni a tutti gli applicativi (ANALISI_INFRASTRUCTURE_RICHIESTE.md §3bis).
     Le richieste stanno tutte in Dip_GG_Richiesta (schema Common). Le pagine "Le mie richieste" (/requests) e
     "Richieste da approvare" (/requestsadmin) le legge RequestService direttamente dalla tabella comune; ogni modulo
     dichiara i propri tipi di richiesta con una classe che implementa IRichiestaTipoHandler (lo scanner dei servizi la
     registra), che da':
        - la strategia di approvazione dell'azienda corrente (chi puo' decidere e in che ordine, letta dalla cronologia
          degli approvatori salvata nella richiesta)
        - i testi della riga (tipo, dettaglio, stato) gia' tradotti con le resx del modulo
        - approva / rifiuta / annulla con gli effetti del modulo: le presenze passano dal proprio motore
          (Dip_GG_RichiestaService.SetState), i tipi approvati dagli amministratori dell'azienda da RichiestaCommonService
     I gestori sono chiamati dentro la richiesta HTTP dell'utente collegato e ricontrollano i permessi nelle azioni.
     Si usano solo i gestori il cui applicativo e' attivo per l'azienda corrente.
     Dopo ogni cambiamento il modulo avvisa gli interessati con IRequestNotifier (SignalR "RequestNotifier").
    */
    public interface IRichiestaTipoHandler
    {
        /// <summary>applicativo del modulo: il gestore si usa solo se e' attivo per l'azienda</summary>
        ApplicationType Application { get; }

        /// <summary>tipi di Dip_GG_Richiesta gestiti</summary>
        IReadOnlyCollection<TipoRichiesta> Tipi { get; }

        /// <summary>strategia di approvazione dell'azienda corrente per questi tipi</summary>
        Task<StrategiaApprovazione> StrategiaAsync();

        /// <summary>completa le righe (testi, SoloData, Extra, Url; puo' correggere CanCancel) gia' preparate da RequestService</summary>
        Task CompletaAsync(List<(Dip_GG_Richiesta Richiesta, RequestItem Item)> righe, RequestScope scope);

        Task ApproveAsync(Dip_GG_Richiesta richiesta);

        /// <summary>motivo facoltativo (lo usa il modulo che lo prevede, es. per avvisare il richiedente)</summary>
        Task RejectAsync(Dip_GG_Richiesta richiesta, string? motivo);

        /// <summary>annullamento (o revoca, se gia' approvata) da parte del richiedente</summary>
        Task CancelAsync(Dip_GG_Richiesta richiesta);
    }

    /// <summary>
    /// Singola: decide un approvatore qualunque (o un amministratore dell'azienda);
    /// Tutti: devono approvare tutti gli approvatori della cronologia; Gerarchia: tutti, nell'ordine della cronologia
    /// </summary>
    public enum StrategiaApprovazione { Singola = 0, Tutti = 1, Gerarchia = 2 }

    public enum RequestScope { Mine = 0, Admin = 1 }

    /// <summary>DaApprovare: per l'amministratore quelle che puo' decidere adesso, per il richiedente quelle in attesa</summary>
    public enum RequestFilter { DaApprovare = 0, Tutte = 1, Approvate = 2 }

    /// <summary>stato comune (per colori e filtri); il testo preciso lo da' il modulo in StatoTesto</summary>
    public enum RequestState { InAttesa = 0, InApprovazione = 1, Approvata = 2, Rifiutata = 3, Annullata = 4 }

    public enum RequestCancelKind { Ritira = 0, ChiediAnnullamento = 1, RitiraAnnullamento = 2 }

    public class RequestCounts
    {
        public int DaApprovare { get; set; }
        public int InAttesa { get; set; }
    }

    public class RequestItem
    {
        /// <summary>chiave per il client: id di Dip_GG_Richiesta (stringa)</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>id di Dip_GG_Richiesta</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>valore di ApplicationType del gestore</summary>
        public int Application { get; set; }

        /// <summary>valore di TipoRichiesta</summary>
        public string Tipo { get; set; } = string.Empty;

        public string TipoTesto { get; set; } = string.Empty;

        public string? Richiedente { get; set; }

        /// <summary>ora locale del server</summary>
        public DateTime Dal { get; set; }

        public DateTime? Al { get; set; }

        /// <summary>Dal/Al sono solo date (senza ora)</summary>
        public bool SoloData { get; set; }

        public string? Dettaglio { get; set; }

        public RequestState Stato { get; set; }

        public string StatoTesto { get; set; } = string.Empty;

        public bool CanApprove { get; set; }

        public bool CanReject { get; set; }

        public bool CanCancel { get; set; }

        /// <summary>
        /// cosa fa "annulla" del richiedente (RichiestaRegole): Ritira la richiesta in attesa, ChiediAnnullamento di quella
        /// approvata, RitiraAnnullamento ancora in attesa (la pratica si chiude, resta approvata)
        /// </summary>
        public RequestCancelKind CancelKind { get; set; }

        /// <summary>pagina del client con il dettaglio (facoltativa) e stato da passarle</summary>
        public string? Url { get; set; }

        public object? UrlState { get; set; }

        /// <summary>codice di un'azione del client fornita dal ramo del modulo (iRequestUiProvider), es. "at.notaSpesa"</summary>
        public string? Extra { get; set; }

        /// <summary>quando e' stata fatta la richiesta, se noto</summary>
        public DateTime? DataRichiesta { get; set; }
    }
}
