using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService.Models
{
    public class Az_SediRepartoUserModel
    {
        
    }

    public class Az_SediRepartoUser_GetAll_InModel
    {

    }

    public class Az_SediRepartoUser_GetAll_OutModel : ModelResult
    {
        public List<Az_SediRepartoUserModel> Az_RepartoUser { get; set; } = new List<Az_SediRepartoUserModel>();

        public Az_SediRepartoUser_GetAll_OutModel()
        {

        }
    }




}
