using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Compliance.Models
{
    /*
     Modelli delle aree di Compliance: sicurezza sul lavoro (CMP-81) e registro GDPR (CMP-GDPR).
     Ogni sezione ha elenco (Cmp_ListInModel -> Cmp_ListOutModel<T>), salvataggio (Cmp_PutInModel<T>) e cancellazione
     (Cmp_DeleteInModel). IdCompany serve solo allo studio (azienda scelta); per gli utenti dell'azienda e' ignorato.
    */

    // ------------------------------------------------------------------------------------------------ generici
    public class Cmp_ListInModel { public int? IdCompany { get; set; } }
    public class Cmp_DeleteInModel { public int Id { get; set; } public int? IdCompany { get; set; } }
    public class Cmp_PutInModel<T> where T : new()
    {
        public T Item { get; set; } = new();
        public int? IdCompany { get; set; }
    }
    public class Cmp_ListOutModel<T> : ModelResult { public List<T> Items { get; set; } = new(); }
    public class Cmp_PutOutModel<T> : ModelResult where T : new() { public T Item { get; set; } = new(); }
    public class Cmp_DeleteOutModel : ModelResult { }

    public class Cmp_VoceModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Attivo { get; set; } = true;
    }

    public class Cmp_PermessiAreeModel
    {
        public bool PowerAdmin { get; set; }
        public bool Admin { get; set; }
        public bool Sanitario { get; set; }
        public bool Studio { get; set; }
    }

    // ------------------------------------------------------------------------------------------------ CMP-81
    public class Cmp_LookupOutModel : ModelResult
    {
        public Cmp_PermessiAreeModel Permessi { get; set; } = new();
        public List<Cmp_VoceModel> Dipendenti { get; set; } = new();
        public List<Cmp_VoceModel> Mansioni { get; set; } = new();
        public List<Cmp_VoceModel> Dpi { get; set; } = new();
        public List<Cmp_VoceModel> Sedi { get; set; } = new();
        public int GiorniPreavviso { get; set; }
    }

    public class Cmp_MansioneModel
    {
        public int Id { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public int Rischio { get; set; }
        public string? Note { get; set; }
        public bool Attivo { get; set; } = true;
        public int NumeroCorsi { get; set; }
        public int NumeroDipendenti { get; set; }
    }

    public class Cmp_MansioneCorsoModel
    {
        public int Id { get; set; }
        public int IdCmp_Mansione { get; set; }
        public string? Mansione { get; set; }
        public int TipoCorso { get; set; }
        public string? Descrizione { get; set; }
        public int? ValiditaMesi { get; set; }
        public int? Ore { get; set; }
    }

    public class Cmp_DipendenteMansioneModel
    {
        public int Id { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
        public int IdCmp_Mansione { get; set; }
        public string? Mansione { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public string? Note { get; set; }
        public bool Attiva { get; set; }
    }

    public class Cmp_FormazioneModel
    {
        public int Id { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
        public int TipoCorso { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public DateTime DataCorso { get; set; }
        public decimal? Ore { get; set; }
        public string? Ente { get; set; }
        public DateTime? DataScadenza { get; set; }
        public string? Attestato { get; set; }
        public string? Note { get; set; }
        // stato calcolato
        public bool Scaduta { get; set; }
        public bool InScadenza { get; set; }
        /// <summary>superato da un corso piu' recente dello stesso tipo (scadenza chiusa)</summary>
        public bool Rinnovata { get; set; }
    }

    public class Cmp_VisitaModel
    {
        public int Id { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
        public int Tipo { get; set; }
        public DateTime DataVisita { get; set; }
        public int Esito { get; set; }
        public string? Limitazioni { get; set; }
        public DateTime? DataScadenza { get; set; }
        public string? Medico { get; set; }
        /// <summary>solo per gli utenti abilitati ai dati sanitari (altrimenti sempre null, anche in ingresso e' ignorato)</summary>
        public string? DatiSanitari { get; set; }
        public bool Scaduta { get; set; }
        public bool InScadenza { get; set; }
        public bool Rinnovata { get; set; }
    }

    public class Cmp_DpiModel
    {
        public int Id { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public int Categoria { get; set; } = 1;
        public string? Norma { get; set; }
        public string? Note { get; set; }
        public bool Attivo { get; set; } = true;
    }

    public class Cmp_DpiConsegnaModel
    {
        public int Id { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
        public int IdCmp_Dpi { get; set; }
        public string? Dpi { get; set; }
        public DateTime DataConsegna { get; set; }
        public int Quantita { get; set; } = 1;
        public string? Taglia { get; set; }
        public string? Note { get; set; }
        /// <summary>presa visione chiesta al dipendente (serve un utente collegato al dipendente)</summary>
        public bool PresaVisioneRichiesta { get; set; }
        public DateTime? DataPresaVisione { get; set; }
    }

    public class Cmp_NominaModel
    {
        public int Id { get; set; }
        public int Tipo { get; set; }
        public int? IdDip_Anagrafica { get; set; }
        public string? NominativoEsterno { get; set; }
        public string? Persona { get; set; }
        public int? IdAz_Sedi { get; set; }
        public string? Sede { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public string? Documento { get; set; }
        public string? Note { get; set; }
        public bool Scaduta { get; set; }
        public bool InScadenza { get; set; }
        public bool PresaVisioneRichiesta { get; set; }
        public DateTime? DataPresaVisione { get; set; }
    }

    public class Cmp_DocumentoModel
    {
        public int Id { get; set; }
        public int Tipo { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string? Versione { get; set; }
        public DateTime DataRevisione { get; set; }
        public DateTime? DataScadenza { get; set; }
        public string? Riferimento { get; set; }
        public string? Note { get; set; }
        public bool Scaduta { get; set; }
        public bool InScadenza { get; set; }
    }

    // situazione della formazione obbligatoria: un corso richiesto dalle mansioni attive di un dipendente
    public class Cmp_FormazioneStatoModel
    {
        public string Id { get; set; } = string.Empty;
        public int IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
        public string? Mansioni { get; set; }
        public int TipoCorso { get; set; }
        public string? Corso { get; set; }
        public int? ValiditaMesi { get; set; }
        public int? IdCmp_Formazione { get; set; }
        public DateTime? DataCorso { get; set; }
        public DateTime? DataScadenza { get; set; }
        /// <summary>0 mancante, 1 valido, 2 in scadenza, 3 scaduto</summary>
        public int Stato { get; set; }
    }

    // pagina del dipendente: i propri corsi, visite (solo esito e scadenza), DPI ricevuti e nomine
    public class Cmp_MineOutModel : ModelResult
    {
        public bool Dipendente { get; set; }
        public List<Cmp_FormazioneModel> Formazione { get; set; } = new();
        public List<Cmp_VisitaModel> Visite { get; set; } = new();
        public List<Cmp_DpiConsegnaModel> Consegne { get; set; } = new();
        public List<Cmp_NominaModel> Nomine { get; set; } = new();
    }

    // ------------------------------------------------------------------------------------------------ CMP-GDPR
    public class Cmp_TrattamentoModel
    {
        public int Id { get; set; }
        public string Denominazione { get; set; } = string.Empty;
        public string? Finalita { get; set; }
        public int BaseGiuridica { get; set; }
        public string? CategorieInteressati { get; set; }
        public string? CategorieDati { get; set; }
        public bool DatiParticolari { get; set; }
        public string? Destinatari { get; set; }
        public string? Trasferimenti { get; set; }
        public string? TerminiCancellazione { get; set; }
        public string? MisureSicurezza { get; set; }
        public string? Note { get; set; }
        public bool Attivo { get; set; } = true;
    }

    public class Cmp_ResponsabileModel
    {
        public int Id { get; set; }
        public int Tipo { get; set; }
        public string Denominazione { get; set; } = string.Empty;
        public string? CodiceFiscale { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Trattamenti { get; set; }
        public DateTime? DataContratto { get; set; }
        public DateTime? DataScadenza { get; set; }
        public string? Documento { get; set; }
        public string? Note { get; set; }
        public bool Scaduta { get; set; }
        public bool InScadenza { get; set; }
    }

    public class Cmp_ViolazioneModel
    {
        public int Id { get; set; }
        public DateTime DataScoperta { get; set; }
        public DateTime? DataViolazione { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public string? CategorieDati { get; set; }
        public int? NumeroInteressati { get; set; }
        public string? Conseguenze { get; set; }
        public string? Misure { get; set; }
        public int Rischio { get; set; } = 1;
        public DateTime? DataNotificaGarante { get; set; }
        public string? MotivoRitardo { get; set; }
        public DateTime? DataComunicazioneInteressati { get; set; }
        public string? Note { get; set; }
        // calcolati: termine delle 72 ore dalla scoperta e stato della notifica
        public DateTime TermineNotifica { get; set; }
        public bool NotificaDovuta { get; set; }
        public bool NotificaInRitardo { get; set; }
    }

    public class Cmp_GdprExportOutModel : ModelResult { public string JobId { get; set; } = string.Empty; }

    // ------------------------------------------------------------------------------------------------ studio
    public class Cmp_StudioAziendaModel
    {
        public int IdCompany { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public int ScadenzeAperte { get; set; }
        public int ScadenzeScadute { get; set; }
    }

    public class Cmp_StudioScadenzaModel
    {
        public int Id { get; set; }
        public string Fonte { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public DateTime DataScadenza { get; set; }
        public bool Chiusa { get; set; }
        public bool Scaduta { get; set; }
        public bool InScadenza { get; set; }
    }
}
