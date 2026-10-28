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
        // nomi scelti per gli amministratori (vuoti = RagioneSociale + "_PowerAdmin" / "_Admin")
        public string? PowerAdminUserName { get; set; } = string.Empty;
        public string? AdminUserName { get; set; } = string.Empty;
    }

    public class CompanyRegisterOutModel : ModelResult
    {
        // indirizzo a cui e' stato inviato il link e sua validita'
        public string Mail { get; set; } = string.Empty;
        public int ScadenzaOre { get; set; }
    }


    // verifica, mentre si compila la pagina, dei nomi degli amministratori che si creeranno
    public class CompanyRegisterUserNamesCheckInModel
    {
        public string? RagioneSociale { get; set; } = string.Empty;
        public string? PowerAdminUserName { get; set; } = string.Empty;
        public string? AdminUserName { get; set; } = string.Empty;
    }

    // errore per ciascun nome (vuoto = valido e libero)
    public class CompanyRegisterUserNamesCheckOutModel : ModelResult
    {
        public string PowerAdminUserNameError { get; set; } = string.Empty;
        public string AdminUserNameError { get; set; } = string.Empty;
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
