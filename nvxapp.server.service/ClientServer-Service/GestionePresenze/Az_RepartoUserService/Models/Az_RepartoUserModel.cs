using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoUserService.Models
{
    public class Az_RepartoUserModel
    {
        
    }

    public class Az_RepartoUser_GetAll_InModel
    {

    }

    public class Az_RepartoUser_GetAll_OutModel : ModelResult
    {
        public List<Az_RepartoUserModel> Az_RepartoUser { get; set; } = new List<Az_RepartoUserModel>();

        public Az_RepartoUser_GetAll_OutModel()
        {

        }
    }




}
