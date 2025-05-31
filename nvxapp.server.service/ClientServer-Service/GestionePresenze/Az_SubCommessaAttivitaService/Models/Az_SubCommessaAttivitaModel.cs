using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models
{
    public class Az_SubCommessaAttivitaModel
    {
        public int Id { get; set; }
        public int IdAz_SubCommessa { get; set; }
        public string? Descrizione { get; set; }
    }

    public class Az_SubCommessaAttivita_GetAll_InModel { }

    public class Az_SubCommessaAttivita_GetAll_OutModel : ModelResult
    {
        public List<Az_SubCommessaAttivitaModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivitaModel>();
    }
}
