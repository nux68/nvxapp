using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_DocsService.Models
{
    /// <summary>documento del modello di export, senza contenuto (elenco)</summary>
    public class Par_ExportCau_DocsModel
    {
        public int Id { get; set; }
        public int IdPar_ExportCau { get; set; }
        public string NomeFile { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int Dimensione { get; set; }
        public string? Descrizione { get; set; }
        public int Ordine { get; set; }
        /// <summary>lunghezza del testo estratto (0 = l'AI non ricevera' nulla, es. PDF scansionato)</summary>
        public int LunghezzaTesto { get; set; }
        public DateTime? CreationDate { get; set; }
    }

    public class Par_ExportCau_Docs_GetAll_InModel
    {
        public int IdPar_ExportCau { get; set; }
    }
    public class Par_ExportCau_Docs_GetAll_OutModel : ModelResult
    {
        public List<Par_ExportCau_DocsModel> Par_ExportCau_Docs { get; set; } = new();
    }

    public class Par_ExportCau_Docs_Get_InModel
    {
        public int Id { get; set; }
    }
    /// <summary>documento con il contenuto (base64) e il testo estratto, per vederlo o scaricarlo</summary>
    public class Par_ExportCau_Docs_Get_OutModel : ModelResult
    {
        public Par_ExportCau_DocsModel? Par_ExportCau_Doc { get; set; }
        public string Contenuto { get; set; } = string.Empty;
        public string? Testo { get; set; }
    }

    public class Par_ExportCau_Docs_Put_InModel
    {
        public int IdPar_ExportCau { get; set; }
        public string NomeFile { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        /// <summary>contenuto del file in base64</summary>
        public string Contenuto { get; set; } = string.Empty;
        public string? Descrizione { get; set; }
    }
    public class Par_ExportCau_Docs_Put_OutModel : ModelResult
    {
        public Par_ExportCau_DocsModel? Par_ExportCau_Doc { get; set; }
    }

    public class Par_ExportCau_Docs_Delete_InModel
    {
        public int Id { get; set; }
    }
    public class Par_ExportCau_Docs_Delete_OutModel : ModelResult { }
}
