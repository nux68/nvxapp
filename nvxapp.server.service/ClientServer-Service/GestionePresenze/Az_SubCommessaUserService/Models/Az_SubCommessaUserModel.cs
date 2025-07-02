using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService.Models
{
    public class Az_SubCommessaUserModel
    {
        public int Id { get; set; }
        public int IdAz_SubCommessa { get; set; }
        public string IdAspNetUsers { get; set; } = string.Empty;
        
    }

    public class Az_SubCommessaUser_GetAll_InModel { }
    public class Az_SubCommessaUser_GetAll_OutModel : ModelResult
    {
        public List<Az_SubCommessaUserModel> Az_SubCommessaUser { get; set; } = new List<Az_SubCommessaUserModel>();
    }



    public class Az_SubCommessaUser4EditModel:Az_SubCommessaUserModel, ICheckObj<int>
    {
        public bool Checked { get; set; }
    }
    public class Az_SubCommessaUser_Get4Commessa_InModel { 
        public int IdAz_Commessa { get; set; }
    }
    public class Az_SubCommessaUser_Get4Commessa_OutModel : ModelResult
    {
        public List<Az_SubCommessaUser4EditModel> Az_SubCommessaUser { get; set; } = new List<Az_SubCommessaUser4EditModel>();
    }

    public class Az_SubCommessaUser_Put4Commessa_InModel { 
        public int IdAz_Commessa { get; set; }
        public List<Az_SubCommessaUser4EditModel> Az_SubCommessaUser { get; set; } = new List<Az_SubCommessaUser4EditModel>();
    }
    public class Az_SubCommessaUser_Put4Commessa_OutModel : ModelResult
    {
        public List<Az_SubCommessaUser4EditModel> Az_SubCommessaUser { get; set; } = new List<Az_SubCommessaUser4EditModel>();
    }

}