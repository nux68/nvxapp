using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models
{
    public class Az_SubCommessaModel
    {
        public int Id { get; set; }
        public int IdAz_Commessa { get; set; }
        public string? Descrizione { get; set; }
        public Boolean Default { get; set; }
        public string Data { get; set; } = string.Empty;
        public string DataA { get; set; } = string.Empty;
    }

    public class Az_SubCommessa_GetAll_InModel { }

    public class Az_SubCommessa_GetAll_OutModel : ModelResult
    {
        public List<Az_SubCommessaModel> Az_SubCommessa { get; set; } = new List<Az_SubCommessaModel>();
    }




    //incorpora tutti i dati che compongono la sub commessa
    public class Az_SubCommessa_4EditModel : Az_SubCommessaModel
    {
        public List<Az_SubCommessaUser4EditModel> Az_SubCommessaUser { get; set; } = new List<Az_SubCommessaUser4EditModel>();
        public List<Az_SubCommessaAttivita4EditModel> Az_SubCommessaAttivita { get; set; } = new List<Az_SubCommessaAttivita4EditModel>();
        public List<Az_SubCommessaSediReparto4EditModel> Az_SubCommessaSediReparto { get; set; } = new List<Az_SubCommessaSediReparto4EditModel>();
    }


    public class Az_SubCommessa_GetAll_4Edit_InModel
    {
        public int Id { get; set; }  // id della commessa
    }

    public class Az_SubCommessa_GetAll_4Edit_OutModel : ModelResult
    {
        public List<Az_SubCommessa_4EditModel> Az_SubCommessa { get; set; } = new List<Az_SubCommessa_4EditModel>();
    }


    public class Az_SubCommessa_PutAll_4Edit_InModel
    {
        public int Id { get; set; }  // id della commessa
        public List<Az_SubCommessa_4EditModel> Az_SubCommessa { get; set; } = new List<Az_SubCommessa_4EditModel>();
    }

    public class Az_SubCommessa_PutAll_4Edit_OutModel : ModelResult
    {
        public int Id { get; set; }  // id della commessa
        public List<Az_SubCommessa_4EditModel> Az_SubCommessa { get; set; } = new List<Az_SubCommessa_4EditModel>();
    }

}
