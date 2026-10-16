using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models
{
    // Verifica, prima di un inserimento, se username o email sono gia' usati da un altro utente
    public class UserCheckInModel
    {
        public string? UserName { get; set; }
        public string? Mail { get; set; }
        // utente da escludere dal controllo (modifica di un utente esistente), vuoto per un nuovo utente
        public string? IdAspNetUsers { get; set; }
    }

    public class UserCheckOutModel : ModelResult
    {
        public bool UserNameExists { get; set; }
        public bool MailExists { get; set; }
    }
}
