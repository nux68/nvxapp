using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService.Models
{
    public class Az_SubCommessaSediRepartoModel
    {
        public int Id { get; set; }
        public int IdAz_SubCommessa { get; set; }
        public int IdAz_SediReparto { get; set; }
    }

    public class Az_SubCommessaSediReparto_GetAll_InModel { }

    public class Az_SubCommessaSediReparto_GetAll_OutModel : ModelResult
    {
        public List<Az_SubCommessaSediRepartoModel> Az_SubCommessaSediReparto { get; set; } = new List<Az_SubCommessaSediRepartoModel>();
    }
}
