using nvxapp.server.data.Entities.Public;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.DocumentManagement
{
    /*
     Documentale (ANALISI_DOCUMENTALE.md §6), schema tenant_<IdAzienda>_4 (o public in modalita' singola).
     Tabelle con prefisso Doc_. Le tabelle comuni (Az_Anagrafica, Az_Cliente, Dip_Anagrafica) stanno nello schema
     Common e sono mappate in sola lettura in DocumentManagementDbContext.
     I file stanno fuori dal database (IFileStorage): qui solo i metadati e la chiave del file.
    */

    // i valori finiscono nel database: non vanno cambiati
    public enum Doc_Categoria
    {
        Personale = 0,   // documenti del personale (cedolini, CU, contratti...): riservati, solo con abilitazione
        Azienda = 1,     // archivio dell'azienda
        Cliente = 2,     // fascicolo del cliente
    }

    public enum Doc_Stato
    {
        Bozza = 0,       // non ancora visibile ai destinatari
        Pubblicato = 1,
        Archiviato = 2,
    }

    public enum Doc_Origine
    {
        Azienda = 0,
        Studio = 1,
        Dipendente = 2,
        Import = 3,
    }

    public enum Doc_LogAzione
    {
        Caricamento = 0,
        NuovaVersione = 1,
        Modifica = 2,
        Visualizzazione = 3,
        Download = 4,
        PresaVisione = 5,
        Cestino = 6,
        Ripristino = 7,
        Eliminazione = 8,
    }

    // configurazione del documentale per l'azienda (una riga)
    public class Doc_Cfg : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        // giorni dopo i quali il cestino si svuota da solo (0 = mai)
        public int GiorniCestino { get; set; } = 30;

        // conserva solo le ultime N versioni di un documento (0 = tutte); quelle con presa visione restano sempre
        public int VersioniDaConservare { get; set; }

        // email ai destinatari quando ricevono un documento (se l'email e' configurata)
        public bool NotificheEmail { get; set; } = true;
    }

    // tipologia di documento
    public class Doc_Par_Tipo : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required, MaxLength(20)]
        public string Codice { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;

        public Doc_Categoria Categoria { get; set; }

        // anni di conservazione (null = senza limite): oltre si segnala, mai eliminazione automatica
        public int? AnniConservazione { get; set; }

        // il documento ha una data di scadenza (es. DURC, certificati)
        public bool HaScadenza { get; set; }

        // ai destinatari si chiede la presa visione
        public bool PresaVisioneRichiesta { get; set; }

        // ha competenza (anno/mese), es. cedolino
        public bool HaCompetenza { get; set; }

        public int Ordine { get; set; }

        public bool Active { get; set; } = true;
    }

    // cartella dell'archivio (albero)
    public class Doc_Cartella : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public int? IdDoc_Cartella { get; set; }
        public Doc_Cartella? Doc_CartellaNavigation { get; set; }
        public ICollection<Doc_Cartella>? Doc_Cartella_Sub { get; set; }

        [Required, MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;

        public int Ordine { get; set; }

        // i documenti aziendali della cartella sono visibili anche agli utenti (dipendenti), in sola lettura
        public bool VisibileUtenti { get; set; }

        // creata dal wizard: non si elimina
        public bool DiSistema { get; set; }
    }

    // abilitazione di un amministratore dell'azienda ai documenti del personale (§4: solo se abilitato esplicitamente)
    public class Doc_UtenteAbilitazione : BaseEntity
    {
        [Required, MaxLength(450)]
        public string IdAspNetUsers { get; set; } = string.Empty;
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        [Required]
        public int IdAz_Anagrafica { get; set; }

        // vede i documenti del personale di tutti (cedolini dei colleghi compresi)
        public bool VedePersonale { get; set; }

        // carica e modifica i documenti del personale
        public bool CaricaPersonale { get; set; }
    }

    // documento
    public class Doc_Documento : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        public int IdDoc_Par_Tipo { get; set; }
        public Doc_Par_Tipo? Doc_Par_TipoNavigation { get; set; }

        public int? IdDoc_Cartella { get; set; }
        public Doc_Cartella? Doc_CartellaNavigation { get; set; }

        // riferimento: dipendente (documenti del personale) o cliente (fascicolo del cliente) o nessuno (aziendale)
        public int? IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public int? IdAz_Cliente { get; set; }
        public Az_Cliente? Az_ClienteNavigation { get; set; }

        [Required, MaxLength(200)]
        public string Titolo { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descrizione { get; set; }

        public DateTime? DataDocumento { get; set; }

        // competenza (es. cedolino di marzo 2026)
        public int? Anno { get; set; }
        public int? Mese { get; set; }

        public DateTime? DataScadenza { get; set; }

        public Doc_Stato Stato { get; set; } = Doc_Stato.Pubblicato;

        public Doc_Origine Origine { get; set; } = Doc_Origine.Azienda;

        // cestino (eliminazione logica): i file restano e contano nello spazio finche' il cestino non si svuota
        public bool NelCestino { get; set; }
        public DateTime? DataCestino { get; set; }

        // numero della versione corrente
        public int VersioneCorrente { get; set; }

        // testo della versione corrente (estratto dal file) e vettore di ricerca su titolo, descrizione e testo
        public string? Testo { get; set; }
        public NpgsqlTsVector? Ricerca { get; set; }

        public ICollection<Doc_Versione>? Doc_Versione { get; set; }
        public ICollection<Doc_Destinatario>? Doc_Destinatario { get; set; }
    }

    // file di una versione del documento
    public class Doc_Versione : BaseEntity
    {
        [Required]
        public int IdDoc_Documento { get; set; }
        public Doc_Documento? Doc_DocumentoNavigation { get; set; }

        public int Numero { get; set; }

        // chiave del file in IFileStorage
        [Required, MaxLength(100)]
        public string StorageKey { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string NomeFile { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long Dimensione { get; set; }

        // SHA-256 esadecimale: integrita' e riconoscimento dei duplicati
        [Required, MaxLength(64)]
        public string Hash { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? IdAspNetUsersAutore { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    // destinatario del documento (dipendente o utente) con la presa visione
    public class Doc_Destinatario : BaseEntity
    {
        [Required]
        public int IdDoc_Documento { get; set; }
        public Doc_Documento? Doc_DocumentoNavigation { get; set; }

        public int? IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        [MaxLength(450)]
        public string? IdAspNetUsers { get; set; }

        public bool PresaVisioneRichiesta { get; set; }

        public DateTime? DataPrimaApertura { get; set; }
        public DateTime? DataPresaVisione { get; set; }
        public DateTime? DataDownload { get; set; }
    }

    // registro degli accessi (obbligatorio per i documenti del personale)
    public class Doc_Log : BaseEntity
    {
        public int? IdDoc_Documento { get; set; }

        public int? Versione { get; set; }

        [MaxLength(450)]
        public string? IdAspNetUsers { get; set; }

        [MaxLength(256)]
        public string? UserName { get; set; }

        public Doc_LogAzione Azione { get; set; }

        public DateTime Data { get; set; }

        [MaxLength(500)]
        public string? Dettaglio { get; set; }

        [MaxLength(64)]
        public string? Ip { get; set; }
    }
}
