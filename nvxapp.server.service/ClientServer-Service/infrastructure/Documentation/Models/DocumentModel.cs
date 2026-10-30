using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Documentation.Models
{
    // livello gestito dall'utente, con i tipi di richiesta proponibili per i documenti di quel livello
    public class DocumentScopeModel
    {
        public TicketLevel Level { get; set; }
        public bool CanEdit { get; set; }
        public List<TicketTypeOptionModel> TicketType { get; set; } = new List<TicketTypeOptionModel>();
    }

    // riga dell'elenco (senza contenuto e senza file)
    public class DocumentListModel
    {
        public int Id { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string? Descrizione { get; set; }
        public string? FileName { get; set; }
        public ApplicationType? ApplicationType { get; set; }
        public bool IncludeSubLevels { get; set; }
        public int Ordine { get; set; }
        public bool Active { get; set; }
        public int ContenutoLength { get; set; }
        public List<int> IdTicketType { get; set; } = new List<int>();
        public DateTime? ModifiedDate { get; set; }
    }

    /*
     Documento in modifica. File viaggia solo quando si carica un nuovo file (RemoveFile toglie quello salvato);
     il contenuto del file salvato si legge con DocumentFileGet.
    */
    public class DocumentEditModel
    {
        public int Id { get; set; }
        public DocumentSection Section { get; set; }
        public TicketLevel Level { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string? Descrizione { get; set; }
        public string Contenuto { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long Size { get; set; }
        public byte[]? File { get; set; }
        public bool RemoveFile { get; set; }
        public ApplicationType? ApplicationType { get; set; }
        public bool IncludeSubLevels { get; set; }
        public int Ordine { get; set; }
        public bool Active { get; set; } = true;
        public List<int> IdTicketType { get; set; } = new List<int>();
        public bool CanEdit { get; set; }
    }


    public class DocumentScopeInModel
    {
        public DocumentSection Section { get; set; }
    }

    public class DocumentScopeOutModel : ModelResult
    {
        public List<DocumentScopeModel> Scope { get; set; } = new List<DocumentScopeModel>();
    }


    public class DocumentListInModel
    {
        public DocumentSection Section { get; set; }
        public TicketLevel Level { get; set; }
    }

    public class DocumentListOutModel : ModelResult
    {
        public List<DocumentListModel> Document { get; set; } = new List<DocumentListModel>();
    }


    public class DocumentGetInModel
    {
        public int Id { get; set; }
    }

    public class DocumentGetOutModel : ModelResult
    {
        public DocumentEditModel Document { get; set; } = new DocumentEditModel();
    }


    public class DocumentPutInModel
    {
        public DocumentEditModel Document { get; set; } = new DocumentEditModel();
    }

    public class DocumentPutOutModel : ModelResult
    {
        public DocumentEditModel Document { get; set; } = new DocumentEditModel();
    }


    public class DocumentDeleteInModel
    {
        public int Id { get; set; }
    }

    public class DocumentDeleteOutModel : ModelResult
    {
    }


    // testo di un file, per vederlo e correggerlo prima di salvare
    public class DocumentFileTextInModel
    {
        public string FileName { get; set; } = string.Empty;
        public byte[]? File { get; set; }
    }

    public class DocumentFileTextOutModel : ModelResult
    {
        public string Text { get; set; } = string.Empty;
    }


    public class DocumentFileGetInModel
    {
        public int Id { get; set; }
    }

    public class DocumentFileGetOutModel : ModelResult
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[]? File { get; set; }
    }
}
