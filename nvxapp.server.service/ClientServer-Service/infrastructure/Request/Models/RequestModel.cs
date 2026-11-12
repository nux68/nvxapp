using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Request.Models
{
    // richieste da approvare comuni a tutti gli applicativi (ANALISI_INFRASTRUCTURE_RICHIESTE.md)

    public class RequestCountInModel
    {
    }

    public class RequestCountOutModel : ModelResult
    {
        // richieste che l'utente (amministratore) puo' approvare adesso
        public int DaApprovare { get; set; }
        // richieste dell'utente in attesa di decisione
        public int InAttesa { get; set; }
    }

    public class RequestListInModel
    {
        // true = pagina "Richieste da approvare" (solo amministratori dell'azienda)
        public bool Admin { get; set; }
        public RequestFilter Filtro { get; set; } = RequestFilter.DaApprovare;
    }

    public class RequestListOutModel : ModelResult
    {
        public List<RequestItem> Richieste { get; set; } = new List<RequestItem>();
        // applicativi che hanno fornito richieste (valori di ApplicationType): il client mostra il filtro se piu' di uno
        public List<int> Applicativi { get; set; } = new List<int>();
        public int DaApprovare { get; set; }
        public int InAttesa { get; set; }
    }

    public class RequestActionInModel
    {
        // id di Dip_GG_Richiesta
        public string Key { get; set; } = string.Empty;
        // solo per il rifiuto
        public string? Motivo { get; set; }
    }

    public class RequestActionOutModel : ModelResult
    {
        public int DaApprovare { get; set; }
        public int InAttesa { get; set; }
    }
}
