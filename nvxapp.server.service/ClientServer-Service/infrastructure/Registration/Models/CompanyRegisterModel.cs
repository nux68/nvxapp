using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Registration.Models
{
    // registrazione di una nuova azienda dalla pagina pubblica (showcase), prima del login:
    // si salva la richiesta e si invia il link di conferma all'email indicata
    public class CompanyRegisterInModel
    {
        public string? RagioneSociale { get; set; } = string.Empty;
        public string? Mail { get; set; } = string.Empty;
        public string? Pw { get; set; } = string.Empty;
        public Boolean AccettaCondizioni { get; set; }
    }

    public class CompanyRegisterOutModel : ModelResult
    {
        // indirizzo a cui e' stato inviato il link e sua validita'
        public string Mail { get; set; } = string.Empty;
        public int ScadenzaOre { get; set; }
    }


    // conferma dal link ricevuto: crea l'azienda con i suoi utenti
    public class CompanyRegisterConfirmInModel
    {
        public string? Token { get; set; } = string.Empty;
    }

    // utente creato per la nuova azienda, da mostrare per il primo accesso
    public class CompanyRegisterUserModel
    {
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class CompanyRegisterConfirmOutModel : ModelResult
    {
        public int IdCompany { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public List<CompanyRegisterUserModel> Users { get; set; } = new List<CompanyRegisterUserModel>();
    }
}
