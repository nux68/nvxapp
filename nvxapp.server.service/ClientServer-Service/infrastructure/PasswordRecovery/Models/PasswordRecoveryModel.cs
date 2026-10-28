using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.PasswordRecovery.Models
{
    // richiesta del link per reimpostare la password (pagina pubblica /forgot-password, prima del login)
    public class PasswordRecoveryRequestInModel
    {
        public string? UserName { get; set; } = string.Empty;
    }

    // la risposta e' sempre la stessa, che l'utente esista o no
    public class PasswordRecoveryRequestOutModel : ModelResult
    {
        public int ScadenzaOre { get; set; }
    }


    // nuova password dal link ricevuto (pagina /reset-password)
    public class PasswordRecoveryResetInModel
    {
        public string? UserId { get; set; } = string.Empty;
        public string? Token { get; set; } = string.Empty;
        public string? Pw { get; set; } = string.Empty;
    }

    public class PasswordRecoveryResetOutModel : ModelResult
    {
        public string UserName { get; set; } = string.Empty;
    }


    // cambio della password conoscendo quella attuale (pagina /change-password, dal login)
    public class PasswordChangeInModel
    {
        public string? UserName { get; set; } = string.Empty;
        public string? Pw { get; set; } = string.Empty;
        public string? NewPw { get; set; } = string.Empty;
    }

    public class PasswordChangeOutModel : ModelResult
    {
        public string UserName { get; set; } = string.Empty;
    }
}
