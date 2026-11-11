using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.Infrastructure.FinancialAdvisorApplication.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models
{
    public class FinancialAdvisorListModel
    {
        public Boolean Disabled { get; set; }
        public int IdFinancialAdvisor { get; set; }

        public string? IdAspNetUsers { get; set; }

        public string? Descrizione { get; set; }
        public Boolean MainUser { get; set; }

    }
    public class FinancialAdvisorListInModel
    {
    }
    public class FinancialAdvisorListOutModel : ModelResult
    {
        public List<FinancialAdvisorListModel> FinancialAdvisorList { get; set; }

        public FinancialAdvisorListOutModel()
        {
            FinancialAdvisorList = new List<FinancialAdvisorListModel>();
        }
    }



    public class FinancialAdvisorEditModel
    {
        // null = non modificare (es. salvataggi da altri flussi che non conoscono il campo)
        public Boolean? Disabled { get; set; }
        public int IdFinancialAdvisor { get; set; }
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string? Descrizione { get; set; } = string.Empty;
        public Boolean MainUser { get; set; }
        public string? Mail { get; set; } = string.Empty;
        public string? Pw { get; set; } = string.Empty;
        // solo in creazione: nomi degli amministratori (vuoti = Descrizione + "_PowerAdmin" / "_Admin")
        public string? PowerAdminUserName { get; set; }
        public string? AdminUserName { get; set; }

        // applicativi abilitati dello studio (funzioni lato studio); null = non modificare
        public List<FinancialAdvisorApplicationModel>? Applications { get; set; }
        // visibili ma non modificabili (li cambiano solo il SuperUser e il DealerPowerAdmin del centro)
        public bool ApplicationsReadOnly { get; set; }
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
