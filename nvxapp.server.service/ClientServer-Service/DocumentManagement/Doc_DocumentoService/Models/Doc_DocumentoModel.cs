using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_DocumentoService.Models
{
    // documento negli elenchi
    public class Doc_DocumentoListModel
    {
        public int Id { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public int IdDoc_Par_Tipo { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public Doc_Categoria Categoria { get; set; }
        public int? IdDoc_Cartella { get; set; }
        public string? Cartella { get; set; }
        public int? IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
        public int? IdAz_Cliente { get; set; }
        public string? Cliente { get; set; }
        public DateTime? DataDocumento { get; set; }
        public int? Anno { get; set; }
        public int? Mese { get; set; }
        public DateTime? DataScadenza { get; set; }
        public Doc_Stato Stato { get; set; }
        public int VersioneCorrente { get; set; }
        public string NomeFile { get; set; } = string.Empty;
        public long Dimensione { get; set; }
        public DateTime? DataCaricamento { get; set; }
        public bool NelCestino { get; set; }
        // per l'utente corrente come destinatario: presa visione richiesta e fatta
        public bool PresaVisioneRichiesta { get; set; }
        public DateTime? DataPresaVisione { get; set; }
        // per l'utente corrente come destinatario: firma richiesta e fatta (sulla versione corrente)
        public bool FirmaRichiesta { get; set; }
        public DateTime? DataFirma { get; set; }
        public bool Nuovo { get; set; }
        // oltre gli anni di conservazione della tipologia (solo segnalazione)
        public bool OltreConservazione { get; set; }
    }

    // invio multiplo: stesso file come documento del personale a piu' dipendenti (un documento per dipendente)
    public class Doc_DocumentoUploadMultiploInModel
    {
        public Doc_DocumentoEditModel Documento { get; set; } = new Doc_DocumentoEditModel();
        public List<int> IdDip_Anagrafica { get; set; } = new List<int>();
    }

    public class Doc_DocumentoUploadMultiploOutModel : ModelResult
    {
        public int Creati { get; set; }
        // un messaggio per ogni dipendente non riuscito (nominativo: motivo)
        public List<string> Errori { get; set; } = new List<string>();
    }

    // dati modificabili di un documento (e del caricamento)
    public class Doc_DocumentoEditModel
    {
        public int Id { get; set; }
        public int IdDoc_Par_Tipo { get; set; }
        public int? IdDoc_Cartella { get; set; }
        public int? IdDip_Anagrafica { get; set; }
        public int? IdAz_Cliente { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string? Descrizione { get; set; }
        public DateTime? DataDocumento { get; set; }
        public int? Anno { get; set; }
        public int? Mese { get; set; }
        public DateTime? DataScadenza { get; set; }
        public Doc_Stato Stato { get; set; } = Doc_Stato.Pubblicato;
        // richiesta di firma ai destinatari: null = quella proposta dalla tipologia; si puo' togliere o aggiungere
        public bool? FirmaRichiesta { get; set; }
        // note della versione (solo caricamento)
        public string? NoteVersione { get; set; }
    }

    public class Doc_VersioneModel
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public string NomeFile { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Dimensione { get; set; }
        public string Hash { get; set; } = string.Empty;
        public DateTime? Data { get; set; }
        public string? Autore { get; set; }
        public string? Note { get; set; }
    }

    public class Doc_DestinatarioModel
    {
        public int Id { get; set; }
        public string Nominativo { get; set; } = string.Empty;
        public bool PresaVisioneRichiesta { get; set; }
        public DateTime? DataPrimaApertura { get; set; }
        public DateTime? DataPresaVisione { get; set; }
        public DateTime? DataDownload { get; set; }
        public bool FirmaRichiesta { get; set; }
        public DateTime? DataFirma { get; set; }
        public int? FirmaVersione { get; set; }
        public string? FirmaHash { get; set; }
        public string? FirmaIp { get; set; }
    }

    public class Doc_LogModel
    {
        public DateTime Data { get; set; }
        public string? UserName { get; set; }
        public Doc_LogAzione Azione { get; set; }
        public int? Versione { get; set; }
        public string? Dettaglio { get; set; }
    }

    public class Doc_DocumentoDettaglioModel
    {
        public Doc_DocumentoEditModel Documento { get; set; } = new Doc_DocumentoEditModel();
        public Doc_DocumentoListModel Riepilogo { get; set; } = new Doc_DocumentoListModel();
        public List<Doc_VersioneModel> Versioni { get; set; } = new List<Doc_VersioneModel>();
        public List<Doc_DestinatarioModel> Destinatari { get; set; } = new List<Doc_DestinatarioModel>();
        // l'utente corrente puo' modificarlo (metadati, nuove versioni, cestino)
        public bool Modificabile { get; set; }
    }

    // ---------------------------------------------------------------- richieste e risposte
    public class Doc_DocumentoListInModel
    {
        public int? IdDoc_Cartella { get; set; }
        public int? IdDoc_Par_Tipo { get; set; }
        public int? IdDip_Anagrafica { get; set; }
        public int? IdAz_Cliente { get; set; }
        public Doc_Categoria? Categoria { get; set; }
        // ricerca nel testo (titolo, descrizione e contenuto dei file)
        public string? Testo { get; set; }
        public int? Anno { get; set; }
        public bool Cestino { get; set; }
        // solo documenti in scadenza entro N giorni (o scaduti)
        public int? ScadenzaEntroGiorni { get; set; }
        // solo i miei documenti (dipendente e destinatario)
        public bool Miei { get; set; }
    }

    public class Doc_DocumentoListOutModel : ModelResult
    {
        public List<Doc_DocumentoListModel> Documenti { get; set; } = new List<Doc_DocumentoListModel>();
        // elenco troncato al massimo di righe: affinare i filtri
        public bool Troncato { get; set; }
    }

    public class Doc_DocumentoGetInModel { public int Id { get; set; } }
    public class Doc_DocumentoGetOutModel : ModelResult { public Doc_DocumentoDettaglioModel Dettaglio { get; set; } = new Doc_DocumentoDettaglioModel(); }

    public class Doc_DocumentoPutInModel { public Doc_DocumentoEditModel Documento { get; set; } = new Doc_DocumentoEditModel(); }
    public class Doc_DocumentoPutOutModel : ModelResult { public Doc_DocumentoDettaglioModel Dettaglio { get; set; } = new Doc_DocumentoDettaglioModel(); }

    public class Doc_DocumentoIdInModel { public int Id { get; set; } }
    public class Doc_DocumentoIdOutModel : ModelResult { public int Id { get; set; } }

    // firma del destinatario: versione che ha davanti (se nel frattempo ne e' arrivata un'altra la firma si rifiuta)
    public class Doc_DocumentoFirmaInModel { public int Id { get; set; } public int Versione { get; set; } }

    public class Doc_SvuotaCestinoInModel { }
    public class Doc_SvuotaCestinoOutModel : ModelResult { public int Eliminati { get; set; } }

    public class Doc_LogListInModel { public int? IdDoc_Documento { get; set; } }
    public class Doc_LogListOutModel : ModelResult { public List<Doc_LogModel> Log { get; set; } = new List<Doc_LogModel>(); }

    // file da scaricare (non e' un GenericResult: il controller lo restituisce come file)
    public sealed class Doc_FileDownload
    {
        public Stream Content { get; init; } = Stream.Null;
        public string NomeFile { get; init; } = string.Empty;
        public string ContentType { get; init; } = "application/octet-stream";
    }
}
