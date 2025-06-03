using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models
{
    public class Az_SubCommessaModel
    {
        public int Id { get; set; }
        public int IdAz_Commessa { get; set; }
        public string? Descrizione { get; set; }
        public Boolean Default { get; set; }
    }

    public class Az_SubCommessa_GetAll_InModel { }

    public class Az_SubCommessa_GetAll_OutModel : ModelResult
    {
        public List<Az_SubCommessaModel> Az_SubCommessa { get; set; } = new List<Az_SubCommessaModel>();
    }
}
