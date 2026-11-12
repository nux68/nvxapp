using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Request
{
    /*
     Richieste da approvare comuni a tutti gli applicativi (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-INF.1).
     Le pagine "Le mie richieste" (/requests) e "Richieste da approvare" (/requestsadmin) sono di Infrastructure: ogni modulo
     porta le proprie richieste con una classe che implementa IRequestProvider (lo scanner dei servizi la registra) e
     RequestService le unisce, chiamando solo i provider il cui applicativo e' attivo per l'azienda corrente.
        - i dati restano nel modulo (es. Dip_GG_Richiesta in Presenze, Ast_Prenotazione in Risorse aziendali)
        - le implementazioni sono chiamate dentro la richiesta HTTP dell'utente collegato (CurrentUserId / CurrentCompany
          come nei servizi del modulo) e ricontrollano LORO i permessi: chi vede, chi approva, chi annulla
        - Id = identificativo della richiesta nel modulo (stringa): la chiave completa per il client e' "<Prefix>:<Id>"
        - testi gia' tradotti (tipo, dettaglio, stato) con le resx del modulo, nella lingua della richiesta
        - dopo ogni cambiamento il modulo avvisa gli interessati con IRequestNotifier (SignalR "RequestNotifier"):
          i client ricalcolano i contatori del menu e ricaricano le pagine aperte
    */
    public interface IRequestProvider
    {
        /// <summary>applicativo del modulo: il provider si chiama solo se e' attivo per l'azienda</summary>
        ApplicationType Application { get; }

        /// <summary>prefisso stabile e unico tra i moduli (es. "at", "ast"), senza ":"</summary>
        string Prefix { get; }

        /// <summary>contatori per l'utente collegato: le sue richieste in attesa e, se admin, quelle che puo' approvare adesso</summary>
        Task<RequestCounts> CountAsync(bool admin);

        /// <summary>richieste dell'utente collegato (Mine) o da gestire come amministratore (Admin), secondo il filtro</summary>
        Task<List<RequestItem>> ListAsync(RequestScope scope, RequestFilter filtro);

        Task ApproveAsync(string id);

        /// <summary>motivo facoltativo (lo usa il modulo che lo prevede, es. per avvisare il richiedente)</summary>
        Task RejectAsync(string id, string? motivo);

        /// <summary>annullamento (o revoca, se gia' approvata) da parte del richiedente</summary>
        Task CancelAsync(string id);
    }

    public enum RequestScope { Mine = 0, Admin = 1 }

    /// <summary>DaApprovare: per l'amministratore quelle che puo' approvare adesso, per il richiedente quelle in attesa</summary>
    public enum RequestFilter { DaApprovare = 0, Tutte = 1, Approvate = 2 }

    /// <summary>stato comune (per colori e filtri); il testo preciso lo da' il modulo in StatoTesto</summary>
    public enum RequestState { InAttesa = 0, InApprovazione = 1, Approvata = 2, Rifiutata = 3, Annullata = 4 }

    public class RequestCounts
    {
        public int DaApprovare { get; set; }
        public int InAttesa { get; set; }
    }

    public class RequestItem
    {
        /// <summary>chiave "<Prefix>:<Id>" (la imposta RequestService)</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>identificativo nel modulo</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>valore di ApplicationType (lo imposta RequestService)</summary>
        public int Application { get; set; }

        /// <summary>codice del tipo nel modulo (es. "giustificativo", "prenotazione")</summary>
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

        /// <summary>pagina del client con il dettaglio (facoltativa) e stato da passarle</summary>
        public string? Url { get; set; }

        public object? UrlState { get; set; }

        /// <summary>codice di un'azione del client fornita dal ramo del modulo (iRequestUiProvider), es. "at.notaSpesa"</summary>
        public string? Extra { get; set; }

        /// <summary>quando e' stata fatta la richiesta, se noto</summary>
        public DateTime? DataRichiesta { get; set; }
    }
}
