using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant.HumanResources
{
    /*
     Cedolini e CU (HR-CED, ANALISI_HUMANRESOURCES_APPLICATIVO.md §4.2), schema tenant_<IdAzienda>_5.
     Lo studio o l'azienda carica il PDF unico (Hr_DocCaricamento); un job lo divide per dipendente (Hr_DocDipendente)
     e mette da parte le pagine non riconosciute (Hr_DocScarto). I file stanno fuori dal database (IFileStorage).
    */

    public enum Hr_TipoDocumento
    {
        Cedolino = 1,
        Cu = 2,
    }

    public enum Hr_OrigineCaricamento
    {
        Azienda = 1,
        Studio = 2,
    }

    public enum Hr_StatoCaricamento
    {
        // il job sta dividendo il PDF
        InElaborazione = 0,
        // ci sono pagine non abbinate da verificare
        DaVerificare = 1,
        // tutte le pagine abbinate (o scartate), documenti non ancora pubblicati
        DaPubblicare = 2,
        // documenti pubblicati nell'area dei dipendenti
        Pubblicato = 3,
        // il PDF non si e' potuto leggere
        Errore = 9,
    }

    public enum Hr_MetodoAbbinamento
    {
        CodiceFiscale = 1,
        Matricola = 2,
        // proposta per cognome e nome trovati nella pagina, confermata a mano
        Nome = 3,
        Manuale = 4,
    }

    public enum Hr_EsitoScarto
    {
        DaVerificare = 0,
        Assegnato = 1,
        Ignorato = 2,
    }

    // un caricamento del PDF unico dei cedolini o delle CU
    public class Hr_DocCaricamento : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public Hr_TipoDocumento Tipo { get; set; } = Hr_TipoDocumento.Cedolino;
        public int Anno { get; set; }
        // mese del cedolino (null per le CU)
        public int? Mese { get; set; }

        public Hr_OrigineCaricamento Origine { get; set; } = Hr_OrigineCaricamento.Azienda;
        // studio che ha caricato (Origine = Studio)
        public int? IdFinancialAdvisor { get; set; }
        [MaxLength(450)]
        public string? IdAspNetUsersCaricamento { get; set; }

        [Required, MaxLength(260)]
        public string NomeFile { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string StorageKey { get; set; } = string.Empty;
        public long Dimensione { get; set; }

        public Hr_StatoCaricamento Stato { get; set; } = Hr_StatoCaricamento.InElaborazione;
        public int NumeroPagine { get; set; }
        [MaxLength(1000)]
        public string? Messaggio { get; set; }
        public DateTime? DataPubblicazione { get; set; }

        public ICollection<Hr_DocDipendente>? Hr_DocDipendente { get; set; }
        public ICollection<Hr_DocScarto>? Hr_DocScarto { get; set; }
    }

    // documento del dipendente estratto dal caricamento (le sue pagine del PDF unico)
    public class Hr_DocDipendente : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }

        [Required]
        public int IdHr_DocCaricamento { get; set; }
        public Hr_DocCaricamento? Hr_DocCaricamentoNavigation { get; set; }

        [Required]
        public int IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public Hr_TipoDocumento Tipo { get; set; }
        public int Anno { get; set; }
        public int? Mese { get; set; }

        // pagine del PDF unico (1, 2, ...) separate da virgola
        [Required, MaxLength(500)]
        public string Pagine { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string StorageKey { get; set; } = string.Empty;
        public long Dimensione { get; set; }

        public Hr_MetodoAbbinamento Metodo { get; set; }
        public DateTime? DataPubblicazione { get; set; }
    }

    // pagina non abbinata da verificare
    public class Hr_DocScarto : BaseEntity
    {
        [Required]
        public int IdHr_DocCaricamento { get; set; }
        public Hr_DocCaricamento? Hr_DocCaricamentoNavigation { get; set; }

        public int Pagina { get; set; }
        // inizio del testo riconosciuto nella pagina
        [MaxLength(2000)]
        public string? Testo { get; set; }
        // proposta (cognome e nome trovati nella pagina) da confermare a mano
        public int? IdDip_AnagraficaProposta { get; set; }
        // motivo dello scarto (nessun codice trovato, piu' dipendenti nella pagina...)
        [MaxLength(200)]
        public string? Motivo { get; set; }

        public Hr_EsitoScarto Esito { get; set; } = Hr_EsitoScarto.DaVerificare;
        public int? IdDip_Anagrafica { get; set; }
    }
}
