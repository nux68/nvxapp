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
    public class CompanyModel
    {
        public int IdCompany { get; set; }

        public required string IdAspNetUsers { get; set; }

        public string? Descrizione { get; set; }

    }
    public class CompanyListInModel
    {
    }
    public class CompanyListOutModel : ModelResult
    {
        public List<CompanyModel> CompanyList { get; set; }

        public CompanyListOutModel()
        {
            CompanyList = new List<CompanyModel>();
        }
    }

    public class CompanyEditModel
    {
        public int IdCompany { get; set; }
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public Boolean MainUser { get; set; }
    }
    public class CompanyGetInModel
    {
        public int Id { get; set; } = 0;
    }
    public class CompanyGetOutModel : ModelResult
    {
        public CompanyEditModel CompanyEdit { get; set; } = new CompanyEditModel();
    }
    public class CompanyPutInModel : ModelResult
    {
        public CompanyEditModel CompanyEdit { get; set; } = new CompanyEditModel();
    }
    public class CompanyPutOutModel : ModelResult
    {
        public CompanyEditModel CompanyEdit { get; set; } = new CompanyEditModel();
    }

}
