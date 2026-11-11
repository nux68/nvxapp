using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Servizi comuni a tutti gli applicativi (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md §5, area INF-SVC):
        - Scadenza          : scadenzario dell'azienda, alimentato dai moduli (corsi, visite, veicoli, documenti...)
        - Notifica          : avvisi conservati per l'utente (nell'app e, se scelto, per email)
        - NotificaPreferenza: come l'utente vuole ricevere una categoria di avvisi
        - PresaVisione      : richiesta a un utente di confermare di aver letto (o firmato) qualcosa
        - CalendarioFeed    : feed iCalendar (.ics) personale dell'utente in un'azienda (area INF-CAL, decisione D9)
     Tabelle in public con IdCompany (come Ticket e UserDashboard): il promemoria notturno legge tutte le aziende
     con una sola query. Chiavi esterne senza navigation: i contesti degli applicativi mappano Company e AspNetUsers
     e non devono scoprire queste tabelle.
     Gli enum si salvano come interi: i valori non vanno rinumerati.
    */

    public enum StatoScadenza
    {
        Aperta = 0,
        Chiusa = 1,
    }

    /// <summary>
    /// Scadenza dell'azienda registrata da un modulo (IDeadlineRegistry). La chiave logica e' modulo + fonte + riferimento:
    /// lo stesso oggetto (es. il corso 12 del dipendente) ha una sola scadenza aperta, aggiornata a ogni salvataggio.
    /// </summary>
    public class Scadenza : BaseEntity
    {
        [Required]
        public int IdCompany { get; set; }

        /// <summary>applicativo che l'ha registrata (valore di ApplicationType; 0 = funzioni base)</summary>
        public int Application { get; set; }

        /// <summary>tipo di oggetto nel modulo (es. "cmp.corso", "ast.veicolo.bollo"): con IdRiferimento identifica la scadenza</summary>
        [Required]
        [MaxLength(50)]
        public string Fonte { get; set; } = string.Empty;

        /// <summary>id dell'oggetto nel modulo (null = scadenza inserita a mano, senza oggetto collegato)</summary>
        public int? IdRiferimento { get; set; }

        [Required]
        [MaxLength(200)]
        public string Descrizione { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Note { get; set; }

        public DateTime DataScadenza { get; set; }

        /// <summary>giorni prima della scadenza in cui parte il promemoria</summary>
        public int GiorniPreavviso { get; set; } = 30;

        /// <summary>utente che riceve i promemoria (null = amministratori dell'azienda)</summary>
        [MaxLength(450)]
        public string? IdAspNetUsersResponsabile { get; set; }

        /// <summary>pagina del client che apre l'oggetto (es. "/cmpcorsoedit") e stato da passarle in JSON (es. {"id":12})</summary>
        [MaxLength(200)]
        public string? Url { get; set; }

        [MaxLength(500)]
        public string? UrlState { get; set; }

        public StatoScadenza Stato { get; set; } = StatoScadenza.Aperta;

        public DateTime? DataChiusura { get; set; }

        /// <summary>promemoria inviati: preavviso (prima della data) e scaduta (dalla data in poi), una volta sola ciascuno</summary>
        public DateTime? DataAvvisoPreavviso { get; set; }
        public DateTime? DataAvvisoScaduta { get; set; }
    }

    /// <summary>
    /// Avviso per un utente (INotificationSender). Si conserva sempre: chi non era collegato lo trova nella pagina delle notifiche.
    /// </summary>
    public class Notifica : BaseEntity
    {
        [Required]
        [MaxLength(450)]
        public string IdAspNetUsers { get; set; } = string.Empty;

        /// <summary>azienda a cui si riferisce (null = avviso non legato a un'azienda, es. lato studio o centro)</summary>
        public int? IdCompany { get; set; }

        /// <summary>categoria (es. "scadenze", "presevisione"): decide le preferenze di invio</summary>
        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Titolo { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Messaggio { get; set; }

        [MaxLength(200)]
        public string? Url { get; set; }

        [MaxLength(500)]
        public string? UrlState { get; set; }

        public bool Letta { get; set; }

        public DateTime? DataLettura { get; set; }

        /// <summary>data dell'invio per email (null = non inviata per email)</summary>
        public DateTime? DataEmail { get; set; }
    }

    /// <summary>
    /// Preferenza dell'utente per una categoria di avvisi. Nessuna riga = valgono i predefiniti della categoria.
    /// </summary>
    public class NotificaPreferenza : BaseEntity
    {
        [Required]
        [MaxLength(450)]
        public string IdAspNetUsers { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty;

        /// <summary>avviso a comparsa nell'app (la notifica si conserva comunque)</summary>
        public bool App { get; set; } = true;

        public bool Email { get; set; }
    }

    /// <summary>
    /// Presa visione richiesta a un utente (IAcknowledgementService). Il canale decide come si conferma: "conferma"
    /// (base: l'utente conferma di aver letto nella pagina delle prese visione) o un canale di un modulo
    /// (es. firma elettronica del documentale).
    /// </summary>
    public class PresaVisione : BaseEntity
    {
        [Required]
        public int IdCompany { get; set; }

        public int Application { get; set; }

        [Required]
        [MaxLength(50)]
        public string Fonte { get; set; } = string.Empty;

        public int? IdRiferimento { get; set; }

        /// <summary>destinatario</summary>
        [Required]
        [MaxLength(450)]
        public string IdAspNetUsers { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Titolo { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Testo { get; set; }

        [MaxLength(200)]
        public string? Url { get; set; }

        [MaxLength(500)]
        public string? UrlState { get; set; }

        /// <summary>canale usato (es. "conferma", "doc.fes") e riferimento nel canale (es. id del destinatario del documento)</summary>
        [Required]
        [MaxLength(50)]
        public string Canale { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdCanale { get; set; }

        public DateTime? DataConferma { get; set; }

        /// <summary>prove della conferma</summary>
        [MaxLength(64)]
        public string? Ip { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>richiesta ritirata dal modulo (non si mostra piu')</summary>
        public bool Annullata { get; set; }
    }

    /// <summary>
    /// Feed iCalendar (.ics) personale in sola lettura (INF-CAL, D9): Google Calendar, Outlook e Apple Calendar lo
    /// sottoscrivono con l'indirizzo che contiene il token, senza autenticazione. Uno per utente e azienda.
    /// Il token e' l'unica protezione: casuale (32 byte, base64url) e rigenerabile (il vecchio smette di funzionare).
    /// </summary>
    public class CalendarioFeed : BaseEntity
    {
        [Required]
        [MaxLength(450)]
        public string IdAspNetUsers { get; set; } = string.Empty;

        [Required]
        public int IdCompany { get; set; }

        [Required]
        [MaxLength(64)]
        public string Token { get; set; } = string.Empty;

        /// <summary>creazione del token attuale (cambia quando si rigenera)</summary>
        public DateTime DataCreazione { get; set; }

        /// <summary>ultimo scaricamento del feed da parte di un calendario</summary>
        public DateTime? DataUltimoAccesso { get; set; }
    }
}
