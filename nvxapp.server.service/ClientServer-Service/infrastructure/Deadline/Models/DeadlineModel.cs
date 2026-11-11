using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline.Models
{
    // scadenzario dell'azienda corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.1-3)
    public class DeadlineModel
    {
        public int Id { get; set; }
        public int Application { get; set; }
        public string Fonte { get; set; } = string.Empty;
        public int? IdRiferimento { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime DataScadenza { get; set; }
        public int GiorniPreavviso { get; set; }
        public string? IdAspNetUsersResponsabile { get; set; }
        public string? Responsabile { get; set; }
        public string? Url { get; set; }
        public string? UrlState { get; set; }
        // 0 = aperta, 1 = chiusa
        public int Stato { get; set; }
        public DateTime? DataChiusura { get; set; }
        // inserita a mano (non da un modulo): si puo' modificare e cancellare
        public bool Manuale { get; set; }
        // aperta e con data passata / entro il preavviso
        public bool Scaduta { get; set; }
        public bool InScadenza { get; set; }
    }

    public class DeadlineUserModel
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }

    public class DeadlineListInModel
    {
        // null = tutte, 0 = aperte, 1 = chiuse
        public int? Stato { get; set; } = 0;
        // solo le scadenze entro n giorni da oggi (comprese quelle scadute); null = tutte
        public int? EntroGiorni { get; set; }
        public string? Testo { get; set; }
        // solo quelle di cui l'utente e' responsabile (per gli amministratori; gli utenti vedono sempre solo le proprie)
        public bool SoloMie { get; set; }
    }

    public class DeadlineListOutModel : ModelResult
    {
        public List<DeadlineModel> Deadline { get; set; } = new List<DeadlineModel>();
        // l'utente vede e gestisce tutte le scadenze dell'azienda
        public bool Amministratore { get; set; }
    }

    public class DeadlineGetInModel
    {
        // 0 = nuova scadenza manuale
        public int Id { get; set; }
    }

    public class DeadlineGetOutModel : ModelResult
    {
        public DeadlineModel Deadline { get; set; } = new DeadlineModel();
        // utenti dell'azienda selezionabili come responsabili
        public List<DeadlineUserModel> Utenti { get; set; } = new List<DeadlineUserModel>();
    }

    // scadenza inserita a mano (Id = 0 nuova)
    public class DeadlinePutInModel
    {
        public int Id { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime DataScadenza { get; set; }
        public int GiorniPreavviso { get; set; } = 30;
        public string? IdAspNetUsersResponsabile { get; set; }
    }

    public class DeadlinePutOutModel : ModelResult
    {
        public DeadlineModel Deadline { get; set; } = new DeadlineModel();
    }

    // chiude (Chiudi = true) o riapre una scadenza
    public class DeadlineCloseInModel
    {
        public int Id { get; set; }
        public bool Chiudi { get; set; } = true;
    }

    public class DeadlineCloseOutModel : ModelResult
    {
        public DeadlineModel Deadline { get; set; } = new DeadlineModel();
    }

    public class DeadlineDeleteInModel
    {
        public int Id { get; set; }
    }

    public class DeadlineDeleteOutModel : ModelResult
    {
    }

    // widget "In scadenza"
    public class DeadlineSummaryInModel
    {
        public int Giorni { get; set; } = 30;
        public int Massimo { get; set; } = 5;
    }

    public class DeadlineSummaryOutModel : ModelResult
    {
        public int Scadute { get; set; }
        public int InScadenza { get; set; }
        public List<DeadlineModel> Prossime { get; set; } = new List<DeadlineModel>();
    }
}
