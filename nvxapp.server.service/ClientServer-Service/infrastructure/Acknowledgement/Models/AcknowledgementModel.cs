using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement.Models
{
    // prese visione dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.6)
    public class AcknowledgementModel
    {
        public int Id { get; set; }
        public int IdCompany { get; set; }
        public string? Azienda { get; set; }
        public int Application { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string? Testo { get; set; }
        public string? Url { get; set; }
        public string? UrlState { get; set; }
        public string Canale { get; set; } = string.Empty;
        // si conferma in questa pagina (canale base); gli altri canali si confermano aprendo Url (es. firma del documento)
        public bool ConfermabileQui { get; set; }
        public DateTime? DataRichiesta { get; set; }
        public DateTime? DataConferma { get; set; }
    }

    public class AcknowledgementListInModel
    {
        // true = solo quelle da confermare
        public bool DaConfermare { get; set; } = true;
    }

    public class AcknowledgementListOutModel : ModelResult
    {
        public List<AcknowledgementModel> Acknowledgement { get; set; } = new List<AcknowledgementModel>();
        public int DaConfermare { get; set; }
    }

    public class AcknowledgementConfirmInModel
    {
        public int Id { get; set; }
    }

    public class AcknowledgementConfirmOutModel : ModelResult
    {
        public AcknowledgementModel Acknowledgement { get; set; } = new AcknowledgementModel();
        public int DaConfermare { get; set; }
    }
}
