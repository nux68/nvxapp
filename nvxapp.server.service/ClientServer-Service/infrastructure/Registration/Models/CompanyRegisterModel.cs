using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Registration.Models
{
    // registrazione di una nuova azienda o di un nuovo studio dalla pagina pubblica (showcase), prima del login:
    // si salva la richiesta e si invia il link di conferma all'email indicata
    public class CompanyRegisterInModel
    {
        // azienda (nello studio DEMO) o studio (nel centro DEMO)
        public TipoRegistrazione Tipo { get; set; } = TipoRegistrazione.Azienda;
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


    // conferma dal link ricevuto: crea l'azienda o lo studio con i suoi utenti
    public class CompanyRegisterConfirmInModel
    {
        public string? Token { get; set; } = string.Empty;
    }

    // utente creato per la nuova azienda o il nuovo studio, da mostrare per il primo accesso
    public class CompanyRegisterUserModel
    {
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class CompanyRegisterConfirmOutModel : ModelResult
    {
        // cosa e' stato creato: l'azienda (IdCompany) o lo studio (IdFinancialAdvisor)
        public TipoRegistrazione Tipo { get; set; }
        public int IdCompany { get; set; }
        public int IdFinancialAdvisor { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public List<CompanyRegisterUserModel> Users { get; set; } = new List<CompanyRegisterUserModel>();
    }
}
