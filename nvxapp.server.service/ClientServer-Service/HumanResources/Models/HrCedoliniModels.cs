using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.HumanResources.Models
{
    // ------------------------------------------------------------------------------------------------ comuni dei cedolini
    // IdCompany: azienda su cui lavora lo studio (pagine lato studio); ignorato per gli utenti dell'azienda
    public class Hr_CedAziendaInModel { public int? IdCompany { get; set; } }
    public class Hr_CedIdInModel : Hr_CedAziendaInModel { public int Id { get; set; } }

    public class Hr_StudioAziendaModel
    {
        public int IdCompany { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }

    public class Hr_StudioAziendeOutModel : ModelResult { public List<Hr_StudioAziendaModel> Aziende { get; set; } = new(); }

    public class Hr_CaricamentoModel
    {
        public int Id { get; set; }
        public int Tipo { get; set; }
        public int Anno { get; set; }
        public int? Mese { get; set; }
        public int Origine { get; set; }
        public string NomeFile { get; set; } = string.Empty;
        public int Stato { get; set; }
        public int NumeroPagine { get; set; }
        public int NumeroDocumenti { get; set; }
        public int NumeroScarti { get; set; }
        public int NumeroPubblicati { get; set; }
        public string? Messaggio { get; set; }
        public DateTime? DataCaricamento { get; set; }
        public DateTime? DataPubblicazione { get; set; }
    }

    public class Hr_CaricamentoListInModel : Hr_CedAziendaInModel { public int? Anno { get; set; } }
    public class Hr_CaricamentoListOutModel : ModelResult { public List<Hr_CaricamentoModel> Caricamenti { get; set; } = new(); }

    // dati del caricamento (form multipart "data" + file)
    public class Hr_CaricamentoUploadModel : Hr_CedAziendaInModel
    {
        public int Tipo { get; set; } = 1;
        public int Anno { get; set; }
        public int? Mese { get; set; }
    }

    public class Hr_CaricamentoUploadOutModel : ModelResult
    {
        public Hr_CaricamentoModel Caricamento { get; set; } = new();
        public string JobId { get; set; } = string.Empty;
    }

    public class Hr_DocDipendenteModel
    {
        public int Id { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string Dipendente { get; set; } = string.Empty;
        public string? CodiceFiscale { get; set; }
        public string? Matricola { get; set; }
        public string Pagine { get; set; } = string.Empty;
        public int Metodo { get; set; }
        public DateTime? DataPubblicazione { get; set; }
        // il dipendente non ha un utente: il documento resta consultabile dall'ufficio del personale
        public bool SenzaUtente { get; set; }
        public DateTime? DataRichiesta { get; set; }
        public DateTime? DataConferma { get; set; }
        public string? Canale { get; set; }
    }

    public class Hr_DocScartoModel
    {
        public int Id { get; set; }
        public int Pagina { get; set; }
        public string? Testo { get; set; }
        public string? Motivo { get; set; }
        public int? IdDip_AnagraficaProposta { get; set; }
        public string? Proposta { get; set; }
        public int Esito { get; set; }
        public int? IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
    }

    // dipendente per le scelte (assegnazione degli scarti, iscrizioni, checklist)
    public class Hr_DipendenteBreveModel
    {
        public int Id { get; set; }
        public string Cognome { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string? Matricola { get; set; }
        public string? CodiceFiscale { get; set; }
        public bool Attivo { get; set; }
    }

    // report delle conferme di presa visione
    public class Hr_ConfermeModel
    {
        public int Pubblicati { get; set; }
        public int Confermati { get; set; }
        public int DaConfermare { get; set; }
        public int SenzaUtente { get; set; }
    }

    public class Hr_CaricamentoGetOutModel : ModelResult
    {
        public Hr_CaricamentoModel Caricamento { get; set; } = new();
        public List<Hr_DocDipendenteModel> Documenti { get; set; } = new();
        public List<Hr_DocScartoModel> Scarti { get; set; } = new();
        public List<Hr_DipendenteBreveModel> Dipendenti { get; set; } = new();
        public Hr_ConfermeModel Conferme { get; set; } = new();
    }

    public class Hr_ScartoAssegnaInModel : Hr_CedAziendaInModel
    {
        public int IdScarto { get; set; }
        public int IdDip_Anagrafica { get; set; }
    }

    public class Hr_JobOutModel : ModelResult { public string JobId { get; set; } = string.Empty; }

    // ------------------------------------------------------------------------------------------------ area del dipendente
    public class Hr_MioDocumentoModel
    {
        public int Id { get; set; }
        public int Tipo { get; set; }
        public int Anno { get; set; }
        public int? Mese { get; set; }
        public DateTime? DataPubblicazione { get; set; }
        // presa visione (pagina comune "Prese visione" o conferma da qui con il canale base)
        public int? IdPresaVisione { get; set; }
        public DateTime? DataConferma { get; set; }
        public bool ConfermabileQui { get; set; }
    }

    public class Hr_MieiDocumentiOutModel : ModelResult
    {
        // l'utente e' collegato a un dipendente dell'azienda
        public bool Dipendente { get; set; }
        public List<Hr_MioDocumentoModel> Documenti { get; set; } = new();
    }

    public class Hr_FileDownload
    {
        public Stream Content { get; set; } = Stream.Null;
        public string NomeFile { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
    }
}
