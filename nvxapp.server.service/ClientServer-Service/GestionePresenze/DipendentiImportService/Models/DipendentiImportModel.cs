using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.DipendentiImportService.Models
{
    /// <summary>dipendente letto dal file: i dati del primo step del wizard di inserimento</summary>
    public class DipendenteImportModel
    {
        public string Cognome { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        /// <summary>data di assunzione "YYYY-MM-DDT00:00:00", vuota se il file non la riporta</summary>
        public string DataAss { get; set; } = string.Empty;
        /// <summary>username o email gia' usati da un altro utente</summary>
        public bool UserNameExists { get; set; }
        public bool MailExists { get; set; }
    }

    public class DipendentiImport_EstraiInModel
    {
        public string NomeFile { get; set; } = string.Empty;
        /// <summary>contenuto del file in base64</summary>
        public string Contenuto { get; set; } = string.Empty;
        /// <summary>indicazioni dell'amministratore per il LLM (facoltative)</summary>
        public string? Note { get; set; }
    }

    public class DipendentiImport_EstraiOutModel : ModelResult
    {
        public List<DipendenteImportModel> Dipendenti { get; set; } = new List<DipendenteImportModel>();
        /// <summary>file troppo lungo: al LLM e' arrivata solo la prima parte del testo</summary>
        public bool TestoTroncato { get; set; }
    }
}
