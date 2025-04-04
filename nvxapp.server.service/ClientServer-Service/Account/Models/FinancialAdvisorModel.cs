using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.ClientServer_Service.Account.Models
{
    public class FinancialAdvisorModel
    {
        public int IdFinancialAdvisor { get; set; }

        public string? IdAspNetUsers { get; set; }

        public string? Descrizione { get; set; }

    }
    public class FinancialAdvisorListInModel
    {
    }
    public class FinancialAdvisorListOutModel : ModelResult
    {
        public List<FinancialAdvisorModel> FinancialAdvisorList { get; set; }

        public FinancialAdvisorListOutModel()
        {
            FinancialAdvisorList = new List<FinancialAdvisorModel>();
        }
    }




    public class FinancialAdvisorEditModel
    {
        public int IdFinancialAdvisor { get; set; }
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public Boolean MainUser { get; set; }
    }
    public class FinancialAdvisorGetInModel
    {
        public int Id { get; set; } = 0;
    }
    public class FinancialAdvisorGetOutModel : ModelResult
    {
        public FinancialAdvisorEditModel FinancialAdvisorEdit { get; set; } = new FinancialAdvisorEditModel();
    }
    public class FinancialAdvisorPutInModel : ModelResult
    {
        public FinancialAdvisorEditModel FinancialAdvisorEdit { get; set; } = new FinancialAdvisorEditModel();
    }
    public class FinancialAdvisorPutOutModel : ModelResult
    {
        public FinancialAdvisorEditModel FinancialAdvisorEdit { get; set; } = new FinancialAdvisorEditModel();
    }


}
