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
}