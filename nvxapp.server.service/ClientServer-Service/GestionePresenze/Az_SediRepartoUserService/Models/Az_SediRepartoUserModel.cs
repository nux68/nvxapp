using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService.Models
{
    public class Az_SediRepartoUserModel
    {
        public int Id { get; set; }
        public int IdAz_SediReparto { get; set; }
        public  string IdAspNetUsers { get; set; } = string.Empty;
        public Boolean EnabledToApproval { get; set; }
        public int ApprovalZOrder { get; set; }
        public DateTime? DataDal { get; set; }
        public DateTime? DataAl { get; set; }
    }

    public class Az_SediRepartoUser_GetAll_InModel
    {
        public int IdAz_SediReparto { get; set; }
    }

    public class Az_SediRepartoUser_GetAll_OutModel : ModelResult
    {
        public List<Az_SediRepartoUserModel> Az_RepartoUser { get; set; } = new List<Az_SediRepartoUserModel>();

        public Az_SediRepartoUser_GetAll_OutModel()
        {

        }
    }




}
