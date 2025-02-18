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
    public class UserCompanyListInModel
    {
    }

    public class UserCompanyListOutModel : ModelResult
    {
        public List<UserCompanyListModel> UserCompanyList { get; set; }

        public UserCompanyListOutModel()
        {
            UserCompanyList = new List<UserCompanyListModel>();
        }
    }

    public class UserCompanyListModel
    {
        
        public required string IdAspNetUsers { get; set; }

        public string? Descrizione { get; set; }
        
    }

}
