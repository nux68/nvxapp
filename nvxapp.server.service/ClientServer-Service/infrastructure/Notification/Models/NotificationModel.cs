using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Notification.Models
{
    // notifiche dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.4)
    public class NotificationModel
    {
        public int Id { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Titolo { get; set; } = string.Empty;
        public string? Messaggio { get; set; }
        public string? Url { get; set; }
        public string? UrlState { get; set; }
        public bool Letta { get; set; }
        public DateTime? Data { get; set; }
    }

    public class NotificationListInModel
    {
        public bool SoloNonLette { get; set; }
        // pagina di 50 notifiche, dalla piu' recente
        public int Pagina { get; set; }
    }

    public class NotificationListOutModel : ModelResult
    {
        public List<NotificationModel> Notification { get; set; } = new List<NotificationModel>();
        public int NonLette { get; set; }
        public bool AltreDisponibili { get; set; }
    }

    public class NotificationCountInModel
    {
    }

    public class NotificationCountOutModel : ModelResult
    {
        public int NonLette { get; set; }
    }

    // segna come lette le notifiche indicate (Tutte = tutte quelle dell'utente)
    public class NotificationReadInModel
    {
        public List<int> Id { get; set; } = new List<int>();
        public bool Tutte { get; set; }
    }

    public class NotificationReadOutModel : ModelResult
    {
        public int NonLette { get; set; }
    }

    // cancella le notifiche indicate (Lette = tutte quelle gia' lette)
    public class NotificationDeleteInModel
    {
        public List<int> Id { get; set; } = new List<int>();
        public bool Lette { get; set; }
    }

    public class NotificationDeleteOutModel : ModelResult
    {
        public int NonLette { get; set; }
    }

    public class NotificationPreferenceModel
    {
        public string Categoria { get; set; } = string.Empty;
        public string Titolo { get; set; } = string.Empty;
        public bool App { get; set; }
        public bool Email { get; set; }
        public bool EmailAllowed { get; set; }
    }

    public class NotificationPreferenceGetInModel
    {
    }

    public class NotificationPreferenceGetOutModel : ModelResult
    {
        public List<NotificationPreferenceModel> Preference { get; set; } = new List<NotificationPreferenceModel>();
        // l'invio delle email e' configurato sul server
        public bool EmailConfigured { get; set; }
    }

    public class NotificationPreferencePutInModel
    {
        public List<NotificationPreferenceModel> Preference { get; set; } = new List<NotificationPreferenceModel>();
    }

    public class NotificationPreferencePutOutModel : ModelResult
    {
        public List<NotificationPreferenceModel> Preference { get; set; } = new List<NotificationPreferenceModel>();
        public bool EmailConfigured { get; set; }
    }
}
