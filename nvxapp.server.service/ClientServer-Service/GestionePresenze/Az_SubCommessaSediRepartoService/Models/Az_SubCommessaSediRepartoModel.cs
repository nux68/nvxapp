using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService.Models
{
    public class Az_SubCommessaSediRepartoModel
    {
        public int Id { get; set; }
        public int IdAz_SubCommessa { get; set; }
        public int IdAz_SediReparto { get; set; }
    }

    public class Az_SubCommessaSediReparto4EditModel : Az_SubCommessaSediRepartoModel, ICheckObj<int>
    {
        public bool Checked { get; set; }
    }

    public class Az_SubCommessaSediReparto_GetAll_InModel { }

    public class Az_SubCommessaSediReparto_GetAll_OutModel : ModelResult
    {
        public List<Az_SubCommessaSediRepartoModel> Az_SubCommessaSediReparto { get; set; } = new List<Az_SubCommessaSediRepartoModel>();
    }

    public class Az_SubCommessaSediReparto_Get4SubCommessa_InModel
    {
        public int IdAz_SubCommessa { get; set; }
    }

    public class Az_SubCommessaSediReparto_Get4SubCommessa_OutModel : ModelResult
    {
        public List<Az_SubCommessaSediReparto4EditModel> Az_SubCommessaSediReparto { get; set; } = new List<Az_SubCommessaSediReparto4EditModel>();
    }

    public class Az_SubCommessaSediReparto_Put4SubCommessa_InModel
    {
        public int IdAz_SubCommessa { get; set; }
        public List<Az_SubCommessaSediReparto4EditModel> Az_SubCommessaSediReparto { get; set; } = new List<Az_SubCommessaSediReparto4EditModel>();
    }

    public class Az_SubCommessaSediReparto_Put4SubCommessa_OutModel : ModelResult
    {
        public List<Az_SubCommessaSediReparto4EditModel> Az_SubCommessaSediReparto { get; set; } = new List<Az_SubCommessaSediReparto4EditModel>();
    }
}
