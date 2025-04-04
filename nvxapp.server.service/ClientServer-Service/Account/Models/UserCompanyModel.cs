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
    public class UserCompanyModel
    {
        public string? IdAspNetUsers { get; set; }

        public string? Descrizione { get; set; }
    }
    public class UserCompanyListInModel
    {
    }
    public class UserCompanyListOutModel : ModelResult
    {
        public List<UserCompanyModel> UserCompanyList { get; set; }

        public UserCompanyListOutModel()
        {
            UserCompanyList = new List<UserCompanyModel>();
        }
    }



    public class UserCompanyEditModel
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
    }
    public class UserCompanyGetInModel
    {
        public string Id { get; set; } = string.Empty ;
    }
    public class UserCompanyGetOutModel : ModelResult
    {
        public UserCompanyEditModel UserCompanyEdit { get; set; } = new UserCompanyEditModel();
    }
    public class UserCompanyPutInModel : ModelResult
    {
        public UserCompanyEditModel UserCompanyEdit { get; set; } = new UserCompanyEditModel();
    }
    public class UserCompanyPutOutModel : ModelResult
    {
        public UserCompanyEditModel UserCompanyEdit { get; set; } = new UserCompanyEditModel();
    }

}
