using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant.HumanResources
{
    /*
     Formazione e competenze (HR-FOR, ANALISI_HUMANRESOURCES_APPLICATIVO.md §4.3), schema tenant_<IdAzienda>_5.
     Catalogo dei corsi, edizioni, partecipanti con l'attestato (file in IFileStorage) e piani di formazione.
     Le competenze sono quelle comuni (Par_Competenza/Dip_Competenza, schema Common).
    */

    public enum Hr_EsitoCorso
    {
        Iscritto = 0,
        Superato = 1,
        NonSuperato = 2,
        Assente = 3,
    }

    // corso del catalogo
    public class Hr_Corso : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required, MaxLength(200)]
        public string Titolo { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? Ente { get; set; }
        public decimal? DurataOre { get; set; }
        // validita' dell'attestato in mesi (null = non scade)
        public int? ValiditaMesi { get; set; }
        // competenza comune acquisita con il corso superato
        public int? IdPar_Competenza { get; set; }
        public Par_Competenza? Par_CompetenzaNavigation { get; set; }
        public bool Obbligatorio { get; set; }
        public bool Attivo { get; set; } = true;
        [MaxLength(2000)]
        public string? Note { get; set; }

        public ICollection<Hr_CorsoEdizione>? Hr_CorsoEdizione { get; set; }
    }

    // edizione di un corso
    public class Hr_CorsoEdizione : BaseEntity
    {
        [Required]
        public int IdHr_Corso { get; set; }
        public Hr_Corso? Hr_CorsoNavigation { get; set; }

        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public int? IdAz_Sedi { get; set; }
        public Az_Sedi? Az_SediNavigation { get; set; }
        [MaxLength(200)]
        public string? Luogo { get; set; }
        [MaxLength(200)]
        public string? Docente { get; set; }
        [MaxLength(2000)]
        public string? Note { get; set; }

        public ICollection<Hr_CorsoPartecipante>? Hr_CorsoPartecipante { get; set; }
    }

    // dipendente iscritto a un'edizione, con esito, scadenza e attestato
    public class Hr_CorsoPartecipante : BaseEntity
    {
        [Required]
        public int IdHr_CorsoEdizione { get; set; }
        public Hr_CorsoEdizione? Hr_CorsoEdizioneNavigation { get; set; }

        [Required]
        public int IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public Hr_EsitoCorso Esito { get; set; } = Hr_EsitoCorso.Iscritto;
        public DateTime? DataConseguimento { get; set; }
        // conseguimento + validita' del corso
        public DateTime? DataScadenza { get; set; }

        [MaxLength(260)]
        public string? AttestatoNomeFile { get; set; }
        [MaxLength(100)]
        public string? AttestatoContentType { get; set; }
        [MaxLength(100)]
        public string? AttestatoStorageKey { get; set; }
        public long? AttestatoDimensione { get; set; }
    }

    // piano di formazione di un anno
    public class Hr_PianoFormazione : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public int Anno { get; set; }
        [Required, MaxLength(200)]
        public string Descrizione { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Note { get; set; }

        public ICollection<Hr_PianoFormazioneRiga>? Hr_PianoFormazioneRiga { get; set; }
    }

    // corso da far seguire a un dipendente entro una data
    public class Hr_PianoFormazioneRiga : BaseEntity
    {
        [Required]
        public int IdHr_PianoFormazione { get; set; }
        public Hr_PianoFormazione? Hr_PianoFormazioneNavigation { get; set; }

        [Required]
        public int IdHr_Corso { get; set; }
        public Hr_Corso? Hr_CorsoNavigation { get; set; }

        [Required]
        public int IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public DateTime? EntroIl { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
