using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.UserDashboard.Models
{
    // dashboard dell'utente corrente nell'azienda corrente (ANALISI_WIDGET.md)
    public class UserDashboardGetInModel
    {
    }

    public class UserDashboardGetOutModel : ModelResult
    {
        // false = mai configurata: il client mostra i widget predefiniti del catalogo
        public bool Configurata { get; set; }
        // chiavi dei widget nell'ordine di visualizzazione
        public List<string> Widgets { get; set; } = new List<string>();
        // l'utente puo' configurare la propria dashboard (ruoli User, CompanyPowerAdmin, CompanyAdmin in un'azienda)
        public bool Configurabile { get; set; }
    }

    // sostituisce l'elenco dei widget dell'utente corrente
    public class UserDashboardPutInModel
    {
        public List<string> Widgets { get; set; } = new List<string>();
    }

    public class UserDashboardPutOutModel : ModelResult
    {
        public bool Configurata { get; set; }
        public List<string> Widgets { get; set; } = new List<string>();
        public bool Configurabile { get; set; }
    }
}
