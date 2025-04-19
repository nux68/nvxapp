using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models
{
    public class UserFinancialAdvisorModel
    {
        public string? IdAspNetUsers { get; set; }
        public int IdUserFinancialAdvisor { get; set; }
        public string? Descrizione { get; set; }
        public Boolean MainUser { get; set; }
        public string? RoleId { get; set; }
    }
    public class UserFinancialAdvisorListInModel
    {
    }
    public class UserFinancialAdvisorListOutModel : ModelResult
    {
        public List<UserFinancialAdvisorModel> UserFinancialAdvisorList { get; set; }

        public UserFinancialAdvisorListOutModel()
        {
            UserFinancialAdvisorList = new List<UserFinancialAdvisorModel>();
        }
    }



    public class UserFinancialAdvisorEditModel
    {
        public int IdUserFinancialAdvisor { get; set; }
        public string? Descrizione { get; set; } = string.Empty;
        public Boolean MainUser { get; set; }
        public string? Mail { get; set; } = string.Empty;
        public string? Pw { get; set; } = string.Empty;
        public string? RoleId { get; set; }
    }
    public class UserFinancialAdvisorGetInModel
    {
        public int Id { get; set; } = 0;
    }
    public class UserFinancialAdvisorGetOutModel : ModelResult
    {
        public UserFinancialAdvisorEditModel UserFinancialAdvisorEdit { get; set; } = new UserFinancialAdvisorEditModel();
    }
    public class UserFinancialAdvisorPutInModel : ModelResult
    {
        public UserFinancialAdvisorEditModel UserFinancialAdvisorEdit { get; set; } = new UserFinancialAdvisorEditModel();
    }
    public class UserFinancialAdvisorPutOutModel : ModelResult
    {
        public UserFinancialAdvisorEditModel UserFinancialAdvisorEdit { get; set; } = new UserFinancialAdvisorEditModel();
    }

}
