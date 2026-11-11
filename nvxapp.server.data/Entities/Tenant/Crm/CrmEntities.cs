using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.Crm
{
    /*
     CRM (ANALISI_CRM_APPLICATIVO.md §5), schema tenant_<IdAzienda>_3 (o public in modalita' singola). Tabelle con prefisso Crm_.
     Le tabelle comuni (Az_Anagrafica, Az_Cliente, Az_ClienteContatto) stanno nello schema Common e sono mappate in
     sola lettura in CrmDbContext: i dati del CRM su un cliente stanno nell'estensione 1-1 Crm_ClienteDati.
     Gli allegati stanno fuori dal database (IFileStorage): qui solo i metadati e la chiave del file.
    */

    // i valori finiscono nel database: non vanno cambiati
    public enum Crm_Visibilita
    {
        SoloPropri = 0,  // i commerciali (User) vedono i clienti di cui sono responsabili, quelli senza responsabile e quelli in cui sono coinvolti
        Tutti = 1,
    }

    public enum Crm_FaseTipo
    {
        Aperta = 0,
        Vinta = 1,
        Persa = 2,
    }

    public enum Crm_StatoCliente
    {
        Prospect = 0,
        Cliente = 1,
        ExCliente = 2,
        NonInteressato = 3,
    }

    public enum Crm_StatoLead
    {
        Nuovo = 0,
        Contattato = 1,
        Qualificato = 2,
        Convertito = 3,
        Scartato = 4,
    }

    public enum Crm_StatoAttivita
    {
        DaFare = 0,
        Fatta = 1,
        Annullata = 2,
    }

    public enum Crm_StatoOfferta
    {
        Bozza = 0,
        Inviata = 1,
        Accettata = 2,
        Rifiutata = 3,
        // validita' passata senza esito (CRM-PREV, ANALISI_CRM_PREVENTIVI.md §3)
        Scaduta = 4,
    }

    // listino del CRM: articoli e servizi (Crm_Par_Prodotto)
    public enum Crm_TipoProdotto
    {
        Articolo = 0,
        Servizio = 1,
    }

    // richieste di assistenza dei clienti dell'azienda (CRM-ASS, ANALISI_CRM_ASSISTENZA.md)
    public enum Crm_StatoRichiesta
    {
        Nuova = 0,
        InCarico = 1,
        InAttesaCliente = 2,
        Risolta = 3,
        Chiusa = 4,
        Annullata = 5,
    }

    public enum Crm_PrioritaRichiesta
    {
        Bassa = 0,
        Normale = 1,
        Alta = 2,
        Urgente = 3,
    }

    public enum Crm_CanaleRichiesta
    {
        Telefono = 0,
        Email = 1,
        DiPersona = 2,
        Sito = 3,
        Altro = 4,
    }

    // tabelle di configurazione (Crm_Par_*): i valori predefiniti li crea l'attivazione
    public enum Crm_ParTabella
    {
        Fase = 0,
        TipoAttivita = 1,
        MotivoPerdita = 2,
        Fonte = 3,
        Prodotto = 4,
        Tag = 5,
    }

    // configurazione del CRM per l'azienda (una riga)
    public class Crm_Cfg : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public Crm_Visibilita Visibilita { get; set; } = Crm_Visibilita.SoloPropri;

        [Required, MaxLength(3)]
        public string Valuta { get; set; } = "EUR";

        // minuti di preavviso dei promemoria delle attivita'
        public int MinutiPreavviso { get; set; } = 30;

        // numerazione delle offerte: prefisso e ultimo numero dell'anno
        [MaxLength(10)]
        public string? OffertaPrefisso { get; set; }
        public int OffertaAnno { get; set; }
        public int OffertaUltimoNumero { get; set; }

        // giorni di validita' predefiniti di un'offerta
        public int OffertaGiorniValidita { get; set; } = 30;

        // testo in fondo al PDF dell'offerta (condizioni, pagamento...)
        [MaxLength(2000)]
        public string? OffertaNote { get; set; }
    }

    public abstract class Crm_ParBase : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required, MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;

        public int Ordine { get; set; }

        public bool Attivo { get; set; } = true;
    }

    // fase della pipeline delle opportunita'
    public class Crm_Par_Fase : Crm_ParBase
    {
        // probabilita' % proposta per le opportunita' in questa fase
        public int Probabilita { get; set; }
        public Crm_FaseTipo Tipo { get; set; } = Crm_FaseTipo.Aperta;
    }

    public class Crm_Par_TipoAttivita : Crm_ParBase
    {
        [MaxLength(50)]
        public string? Icona { get; set; }
        [MaxLength(20)]
        public string? Colore { get; set; }
        public int DurataMinuti { get; set; } = 30;
    }

    public class Crm_Par_MotivoPerdita : Crm_ParBase { }

    public class Crm_Par_Fonte : Crm_ParBase { }

    public class Crm_Par_Prodotto : Crm_ParBase
    {
        [MaxLength(30)]
        public string? Codice { get; set; }
        [Column(TypeName = "numeric(18,4)")]
        public decimal Prezzo { get; set; }
        [MaxLength(10)]
        public string? Um { get; set; }
        [Column(TypeName = "numeric(5,2)")]
        public decimal AliquotaIva { get; set; } = 22;
        public Crm_TipoProdotto TipoProdotto { get; set; } = Crm_TipoProdotto.Articolo;
    }

    public class Crm_Par_Tag : Crm_ParBase
    {
        [MaxLength(20)]
        public string? Colore { get; set; }
    }

    // dati del CRM su un cliente comune (estensione 1-1 di Az_Cliente)
    public class Crm_ClienteDati : BaseEntity
    {
        [Required]
        public int IdAz_Cliente { get; set; }
        public Az_Cliente? Az_ClienteNavigation { get; set; }

        public Crm_StatoCliente Stato { get; set; } = Crm_StatoCliente.Prospect;

        public int? IdCrm_Par_Fonte { get; set; }
        public Crm_Par_Fonte? Crm_Par_FonteNavigation { get; set; }

        // commerciale responsabile del cliente
        [MaxLength(450)]
        public string? IdAspNetUsersResponsabile { get; set; }

        [MaxLength(100)]
        public string? Settore { get; set; }
        [Column(TypeName = "numeric(18,2)")]
        public decimal? FatturatoStimato { get; set; }
        public int? NumeroDipendenti { get; set; }
        [MaxLength(200)]
        public string? Sito { get; set; }

        // GDPR: consenso alle comunicazioni commerciali
        public bool ConsensoMarketing { get; set; }
        public DateTime? DataConsenso { get; set; }
    }

    public class Crm_ClienteTag : BaseEntity
    {
        [Required]
        public int IdAz_Cliente { get; set; }
        public Az_Cliente? Az_ClienteNavigation { get; set; }

        [Required]
        public int IdCrm_Par_Tag { get; set; }
        public Crm_Par_Tag? Crm_Par_TagNavigation { get; set; }
    }

    // contatto non ancora qualificato: con la conversione diventa cliente (+ contatto, + opportunita')
    public class Crm_Lead : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [MaxLength(100)]
        public string? Cognome { get; set; }
        [MaxLength(100)]
        public string? Nome { get; set; }
        [MaxLength(100)]
        public string? Azienda { get; set; }
        [MaxLength(100)]
        public string? Ruolo { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        [MaxLength(30)]
        public string? Telefono { get; set; }

        public int? IdCrm_Par_Fonte { get; set; }
        public Crm_Par_Fonte? Crm_Par_FonteNavigation { get; set; }

        public Crm_StatoLead Stato { get; set; } = Crm_StatoLead.Nuovo;

        [MaxLength(450)]
        public string? IdAspNetUsersResponsabile { get; set; }

        [MaxLength(2000)]
        public string? Note { get; set; }

        public bool ConsensoMarketing { get; set; }
        public DateTime? DataConsenso { get; set; }

        // dopo la conversione
        public int? IdAz_Cliente { get; set; }
        public Az_Cliente? Az_ClienteNavigation { get; set; }
        public DateTime? DataConversione { get; set; }
    }

    // trattativa
    public class Crm_Opportunita : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required, MaxLength(200)]
        public string Titolo { get; set; } = string.Empty;

        [Required]
        public int IdAz_Cliente { get; set; }
        public Az_Cliente? Az_ClienteNavigation { get; set; }

        public int? IdAz_ClienteContatto { get; set; }
        public Az_ClienteContatto? Az_ClienteContattoNavigation { get; set; }

        [Required]
        public int IdCrm_Par_Fase { get; set; }
        public Crm_Par_Fase? Crm_Par_FaseNavigation { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal Valore { get; set; }
        public int Probabilita { get; set; }

        public DateTime? DataChiusuraPrevista { get; set; }
        public DateTime? DataChiusura { get; set; }

        [MaxLength(450)]
        public string? IdAspNetUsersResponsabile { get; set; }

        public int? IdCrm_Par_MotivoPerdita { get; set; }
        public Crm_Par_MotivoPerdita? Crm_Par_MotivoPerditaNavigation { get; set; }

        public int? IdCrm_Par_Fonte { get; set; }
        public Crm_Par_Fonte? Crm_Par_FonteNavigation { get; set; }

        [MaxLength(2000)]
        public string? Note { get; set; }

        // commessa delle presenze creata dall'opportunita' vinta (ID senza FK: altro applicativo)
        public int? IdCommessaPresenze { get; set; }

        public int? IdCrm_Lead { get; set; }
    }

    // cambi di fase e di valore delle opportunita' (report di conversione)
    public class Crm_OpportunitaStorico : BaseEntity
    {
        [Required]
        public int IdCrm_Opportunita { get; set; }
        public Crm_Opportunita? Crm_OpportunitaNavigation { get; set; }

        public DateTime Data { get; set; }
        [MaxLength(450)]
        public string? IdAspNetUsers { get; set; }

        public int? IdCrm_Par_FaseDa { get; set; }
        public int IdCrm_Par_FaseA { get; set; }
        [Column(TypeName = "numeric(18,2)")]
        public decimal? ValoreDa { get; set; }
        [Column(TypeName = "numeric(18,2)")]
        public decimal ValoreA { get; set; }
    }

    // chiamata, appuntamento, promemoria...
    public class Crm_Attivita : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        public int IdCrm_Par_TipoAttivita { get; set; }
        public Crm_Par_TipoAttivita? Crm_Par_TipoAttivitaNavigation { get; set; }

        [Required, MaxLength(200)]
        public string Oggetto { get; set; } = string.Empty;
        [MaxLength(4000)]
        public string? Descrizione { get; set; }

        public DateTime Inizio { get; set; }
        public DateTime? Fine { get; set; }

        [Required, MaxLength(450)]
        public string IdAspNetUsersAssegnatario { get; set; } = string.Empty;

        public Crm_StatoAttivita Stato { get; set; } = Crm_StatoAttivita.DaFare;
        [MaxLength(2000)]
        public string? Esito { get; set; }

        public int? IdAz_Cliente { get; set; }
        public Az_Cliente? Az_ClienteNavigation { get; set; }
        public int? IdAz_ClienteContatto { get; set; }
        public Az_ClienteContatto? Az_ClienteContattoNavigation { get; set; }
        public int? IdCrm_Opportunita { get; set; }
        public Crm_Opportunita? Crm_OpportunitaNavigation { get; set; }
        public int? IdCrm_Lead { get; set; }
        public Crm_Lead? Crm_LeadNavigation { get; set; }

        // promemoria gia' inviato (SignalR)
        public bool PromemoriaInviato { get; set; }
    }

    public class Crm_Offerta : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public int Anno { get; set; }
        public int Numero { get; set; }

        public DateTime Data { get; set; }
        public DateTime? ValidaFinoAl { get; set; }

        public Crm_StatoOfferta Stato { get; set; } = Crm_StatoOfferta.Bozza;

        [Required]
        public int IdAz_Cliente { get; set; }
        public Az_Cliente? Az_ClienteNavigation { get; set; }
        public int? IdAz_ClienteContatto { get; set; }
        public Az_ClienteContatto? Az_ClienteContattoNavigation { get; set; }

        public int? IdCrm_Opportunita { get; set; }
        public Crm_Opportunita? Crm_OpportunitaNavigation { get; set; }

        [MaxLength(200)]
        public string? Oggetto { get; set; }
        [MaxLength(4000)]
        public string? Note { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal Imponibile { get; set; }
        [Column(TypeName = "numeric(18,2)")]
        public decimal Iva { get; set; }
        [Column(TypeName = "numeric(18,2)")]
        public decimal Totale { get; set; }

        public DateTime? DataInvio { get; set; }

        // accettata, rifiutata o scaduta
        public DateTime? DataEsito { get; set; }

        // predisposizione: ordine creato nell'Amministrazione (applicativo 8, piu' avanti); ID senza FK (altro applicativo)
        public int? IdOrdineAmministrazione { get; set; }

        public List<Crm_OffertaRiga> Righe { get; set; } = new();
    }

    public class Crm_OffertaRiga : BaseEntity
    {
        [Required]
        public int IdCrm_Offerta { get; set; }
        public Crm_Offerta? Crm_OffertaNavigation { get; set; }

        public int Ordine { get; set; }

        public int? IdCrm_Par_Prodotto { get; set; }
        public Crm_Par_Prodotto? Crm_Par_ProdottoNavigation { get; set; }

        [Required, MaxLength(500)]
        public string Descrizione { get; set; } = string.Empty;
        [Column(TypeName = "numeric(18,4)")]
        public decimal Quantita { get; set; } = 1;
        [MaxLength(10)]
        public string? Um { get; set; }
        [Column(TypeName = "numeric(18,4)")]
        public decimal Prezzo { get; set; }
        [Column(TypeName = "numeric(5,2)")]
        public decimal ScontoPerc { get; set; }
        [Column(TypeName = "numeric(5,2)")]
        public decimal AliquotaIva { get; set; }
        [Column(TypeName = "numeric(18,2)")]
        public decimal Importo { get; set; }
    }

    // nota libera su un cliente, un'opportunita' o un lead
    public class Crm_Nota : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public int? IdAz_Cliente { get; set; }
        public Az_Cliente? Az_ClienteNavigation { get; set; }
        public int? IdCrm_Opportunita { get; set; }
        public Crm_Opportunita? Crm_OpportunitaNavigation { get; set; }
        public int? IdCrm_Lead { get; set; }
        public Crm_Lead? Crm_LeadNavigation { get; set; }

        [Required, MaxLength(4000)]
        public string Testo { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? IdAspNetUsersAutore { get; set; }

        // fissata in alto
        public bool Fissata { get; set; }
    }

    // file di un cliente, un'opportunita' o un'offerta (sempre qui, anche con il documentale attivo)
    public class Crm_Allegato : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public int? IdAz_Cliente { get; set; }
        public Az_Cliente? Az_ClienteNavigation { get; set; }
        public int? IdCrm_Opportunita { get; set; }
        public Crm_Opportunita? Crm_OpportunitaNavigation { get; set; }
        public int? IdCrm_Offerta { get; set; }
        public Crm_Offerta? Crm_OffertaNavigation { get; set; }

        [Required, MaxLength(260)]
        public string NomeFile { get; set; } = string.Empty;
        [Required, MaxLength(150)]
        public string ContentType { get; set; } = "application/octet-stream";
        public long Dimensione { get; set; }
        [Required, MaxLength(300)]
        public string StorageKey { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? IdAspNetUsersAutore { get; set; }
    }

    // richiesta di assistenza di un cliente dell'azienda (diversa dai ticket di Infrastructure verso la piattaforma)
    public class Crm_Richiesta : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public int Anno { get; set; }
        public int Numero { get; set; }

        [Required]
        public int IdAz_Cliente { get; set; }
        public Az_Cliente? Az_ClienteNavigation { get; set; }
        public int? IdAz_ClienteContatto { get; set; }
        public Az_ClienteContatto? Az_ClienteContattoNavigation { get; set; }

        [Required, MaxLength(200)]
        public string Oggetto { get; set; } = string.Empty;
        [MaxLength(4000)]
        public string? Descrizione { get; set; }

        public Crm_CanaleRichiesta Canale { get; set; } = Crm_CanaleRichiesta.Telefono;
        public Crm_PrioritaRichiesta Priorita { get; set; } = Crm_PrioritaRichiesta.Normale;
        public Crm_StatoRichiesta Stato { get; set; } = Crm_StatoRichiesta.Nuova;

        [MaxLength(450)]
        public string? IdAspNetUsersAssegnatario { get; set; }

        public DateTime DataApertura { get; set; }
        public DateTime? DataPresaInCarico { get; set; }
        public DateTime? DataRisoluzione { get; set; }
        public DateTime? DataChiusura { get; set; }

        [MaxLength(2000)]
        public string? Soluzione { get; set; }

        public int? IdCrm_Opportunita { get; set; }
        public Crm_Opportunita? Crm_OpportunitaNavigation { get; set; }
    }

    // nota interna o evento (assegnazione, cambio di stato) di una richiesta di assistenza
    public class Crm_RichiestaMessaggio : BaseEntity
    {
        [Required]
        public int IdCrm_Richiesta { get; set; }
        public Crm_Richiesta? Crm_RichiestaNavigation { get; set; }

        public DateTime Data { get; set; }

        [MaxLength(450)]
        public string? IdAspNetUsersAutore { get; set; }

        [Required, MaxLength(4000)]
        public string Testo { get; set; } = string.Empty;

        // evento registrato dal sistema (non modificabile)
        public bool Sistema { get; set; }
    }
}
