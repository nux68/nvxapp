using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models
{
    public class Az_SubCommessaAttivitaModel
    {
        public int Id { get; set; }
        public int IdAz_SubCommessa { get; set; }
        public int IdPar_Attivita { get; set; }
        public Boolean Default { get; set; }
    }



    public class Az_SubCommessaAttivita_GetAll_InModel { }

    public class Az_SubCommessaAttivita_GetAll_OutModel : ModelResult
    {
        public List<Az_SubCommessaAttivitaModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivitaModel>();
    }

    public class Az_SubCommessaAttivita4EditModel : Az_SubCommessaAttivitaModel, ICheckObj<int>
    {
        public bool Checked { get; set; }
    }

    public class Az_SubCommessaAttivita_Get4SubCommessa_InModel
    {
        public int IdAz_SubCommessa { get; set; }
    }

    public class Az_SubCommessaAttivita_Get4SubCommessa_OutModel : ModelResult
    {
        public List<Az_SubCommessaAttivita4EditModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivita4EditModel>();
    }

    public class Az_SubCommessaAttivita_Put4SubCommessa_InModel
    {
        public int IdAz_SubCommessa { get; set; }
        public List<Az_SubCommessaAttivita4EditModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivita4EditModel>();
    }

    public class Az_SubCommessaAttivita_Put4SubCommessa_OutModel : ModelResult
    {
        public List<Az_SubCommessaAttivita4EditModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivita4EditModel>();
    }
}
