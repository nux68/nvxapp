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
        /// <summary>matricola paghe e codice fiscale, vuoti se il file non li riporta</summary>
        public string Matricola { get; set; } = string.Empty;
        public string CodiceFiscale { get; set; } = string.Empty;
        /// <summary>username o email gia' usati da un altro utente</summary>
        public bool UserNameExists { get; set; }
        public bool MailExists { get; set; }
        /// <summary>matricola o codice fiscale gia' usati da un altro dipendente dell'azienda; codice fiscale non valido</summary>
        public bool MatricolaExists { get; set; }
        public bool CodiceFiscaleExists { get; set; }
        public bool CodiceFiscaleNonValido { get; set; }
    }

    public class DipendentiImport_EstraiInModel
    {
        public string NomeFile { get; set; } = string.Empty;
        /// <summary>contenuto del file in base64</summary>
        public string Contenuto { get; set; } = string.Empty;
        /// <summary>indicazioni dell'amministratore per il LLM (facoltative)</summary>
        public string? Note { get; set; }
    }

    /// <summary>dipendente da inserire, con i dati corretti nel dialog</summary>
    public class DipendenteImportDaInserireModel
    {
        public string Cognome { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        /// <summary>data di assunzione (anche inizio del profilo orario); null = oggi</summary>
        public DateTime? DataAss { get; set; }
        public string? Matricola { get; set; }
        public string? CodiceFiscale { get; set; }
        public int IdPar_ProfiloOrario { get; set; }
    }

    public class DipendentiImport_EseguiInModel
    {
        /// <summary>password iniziale, uguale per tutti</summary>
        public string Password { get; set; } = string.Empty;
        public List<DipendenteImportDaInserireModel> Dipendenti { get; set; } = new List<DipendenteImportDaInserireModel>();
    }

    public class DipendentiImport_EseguiOutModel : ModelResult
    {
        /// <summary>job avviato: avanzamento e fine arrivano con le notifiche LongJobProgress</summary>
        public string JobId { get; set; } = string.Empty;
    }

    /// <summary>esito del job, nel Payload della notifica di fine</summary>
    public class DipendentiImport_EsitoModel
    {
        public int Inseriti { get; set; }
        public int Totale { get; set; }
        public List<string> Errori { get; set; } = new List<string>();
    }

    public class DipendentiImport_EstraiOutModel : ModelResult
    {
        public List<DipendenteImportModel> Dipendenti { get; set; } = new List<DipendenteImportModel>();
        /// <summary>file troppo lungo: al LLM e' arrivata solo la prima parte del testo</summary>
        public bool TestoTroncato { get; set; }
    }
}
