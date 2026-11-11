using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant.Compliance
{
    /*
     Sicurezza sul lavoro, D.Lgs 81/08 (CMP-81, ANALISI_COMPLIANCE_APPLICATIVO.md §4.2), schema tenant_<IdAzienda>_6.
     Dipendenti e sedi stanno in Common (Dip_Anagrafica, Az_Sedi) e sono letti in sola lettura dal contesto di Compliance.
     I codici (rischio, tipo di corso, tipo ed esito di visita, tipo di nomina e di documento) sono interi: le descrizioni
     sono nelle traduzioni del client (cmp.enums.*) e nei testi del server (ComplianceMessages).
    */

    // mansione con il livello di rischio (0 basso, 1 medio, 2 alto) e i corsi obbligatori collegati (Cmp_MansioneCorso)
    public class Cmp_Mansione : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;
        public int Rischio { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
        public bool Attivo { get; set; } = true;
    }

    // corso richiesto per la mansione: tipo di corso e validita' in mesi (null = senza scadenza)
    public class Cmp_MansioneCorso : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        public int IdCmp_Mansione { get; set; }
        public Cmp_Mansione? Cmp_MansioneNavigation { get; set; }

        public int TipoCorso { get; set; }
        [MaxLength(100)]
        public string? Descrizione { get; set; }
        public int? ValiditaMesi { get; set; }
        public int? Ore { get; set; }
    }

    // mansione assegnata al dipendente (dal / al)
    public class Cmp_DipendenteMansione : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        public int IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        [Required]
        public int IdCmp_Mansione { get; set; }
        public Cmp_Mansione? Cmp_MansioneNavigation { get; set; }

        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    // corso svolto dal dipendente; la scadenza va nello scadenzario comune (fonte cmp.formazione)
    public class Cmp_Formazione : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        public int IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public int TipoCorso { get; set; }
        [Required]
        [MaxLength(200)]
        public string Descrizione { get; set; } = string.Empty;
        public DateTime DataCorso { get; set; }
        public decimal? Ore { get; set; }
        [MaxLength(100)]
        public string? Ente { get; set; }
        public DateTime? DataScadenza { get; set; }
        // riferimento all'attestato (protocollo, posizione nel documentale...): il file non si carica qui
        [MaxLength(200)]
        public string? Attestato { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /*
     Visita medica: tipo, esito di idoneita' (0 idoneo, 1 idoneo con prescrizioni/limitazioni, 2 temporaneamente non idoneo,
     3 permanentemente non idoneo), sintesi delle limitazioni, scadenza (fonte cmp.visita).
     DatiSanitari: note sanitarie CIFRATE (DataProtection), visibili solo agli utenti abilitati in Cmp_Cfg.UtentiDatiSanitari
     (medico competente / RSPP designato), mai nella chat AI ne' negli export.
    */
    public class Cmp_Visita : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        public int IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public int Tipo { get; set; }
        public DateTime DataVisita { get; set; }
        public int Esito { get; set; }
        [MaxLength(500)]
        public string? Limitazioni { get; set; }
        public DateTime? DataScadenza { get; set; }
        [MaxLength(100)]
        public string? Medico { get; set; }
        public string? DatiSanitari { get; set; }
    }

    // catalogo dei DPI (categoria 1, 2, 3 del Reg. UE 2016/425)
    public class Cmp_Dpi : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;
        public int Categoria { get; set; } = 1;
        [MaxLength(50)]
        public string? Norma { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
        public bool Attivo { get; set; } = true;
    }

    // consegna di un DPI al dipendente: il verbale e' una presa visione (IAcknowledgementService, fonte cmp.dpi)
    public class Cmp_DpiConsegna : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        public int IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        [Required]
        public int IdCmp_Dpi { get; set; }
        public Cmp_Dpi? Cmp_DpiNavigation { get; set; }

        public DateTime DataConsegna { get; set; }
        public int Quantita { get; set; } = 1;
        [MaxLength(20)]
        public string? Taglia { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    // nomina (RSPP, ASPP, RLS, preposto, addetti...): persona interna (dipendente) o esterna, sede facoltativa, scadenza = DataFine
    public class Cmp_Nomina : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public int Tipo { get; set; }
        public int? IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }
        [MaxLength(100)]
        public string? NominativoEsterno { get; set; }
        public int? IdAz_Sedi { get; set; }
        public Az_Sedi? Az_SediNavigation { get; set; }

        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        // riferimento al documento di nomina (protocollo, posizione nel documentale...)
        [MaxLength(200)]
        public string? Documento { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    // DVR, piano di emergenza, DUVRI, procedure: versione, revisione e scadenza (fonte cmp.documento)
    public class Cmp_Documento : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public int Tipo { get; set; }
        [Required]
        [MaxLength(200)]
        public string Titolo { get; set; } = string.Empty;
        [MaxLength(20)]
        public string? Versione { get; set; }
        public DateTime DataRevisione { get; set; }
        public DateTime? DataScadenza { get; set; }
        // riferimento al file (protocollo, posizione nel documentale...)
        [MaxLength(200)]
        public string? Riferimento { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
