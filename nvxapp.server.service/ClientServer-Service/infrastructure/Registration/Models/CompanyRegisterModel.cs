using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Registration.Models
{
    // registrazione di una nuova azienda dalla pagina pubblica (showcase), prima del login
    public class CompanyRegisterInModel
    {
        public string? RagioneSociale { get; set; } = string.Empty;
        public string? Mail { get; set; } = string.Empty;
        public string? Pw { get; set; } = string.Empty;
        public Boolean AccettaCondizioni { get; set; }
    }

    // utente creato per la nuova azienda, da mostrare per il primo accesso
    public class CompanyRegisterUserModel
    {
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class CompanyRegisterOutModel : ModelResult
    {
        public int IdCompany { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public List<CompanyRegisterUserModel> Users { get; set; } = new List<CompanyRegisterUserModel>();
    }
}
