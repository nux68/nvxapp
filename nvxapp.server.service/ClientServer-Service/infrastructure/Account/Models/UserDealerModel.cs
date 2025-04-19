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
    public class UserDealerModel
    {
        public string? IdAspNetUsers { get; set; }
        public int IdUserDealer { get; set; }
        public string? Descrizione { get; set; }
        public Boolean MainUser { get; set; }
        public string? RoleId { get; set; }
    }
    public class UserDealerListInModel
    {
    }
    public class UserDealerListOutModel : ModelResult
    {
        public List<UserDealerModel> UserDealerList { get; set; }

        public UserDealerListOutModel()
        {
            UserDealerList = new List<UserDealerModel>();
        }
    }



    public class UserDealerEditModel
    {
        public int IdUserDealer { get; set; }
        public string? Descrizione { get; set; } = string.Empty;
        public Boolean MainUser { get; set; }
        public string? Mail { get; set; } = string.Empty;
        public string? Pw { get; set; } = string.Empty;
        public string? RoleId { get; set; }
    }
    public class UserDealerGetInModel
    {
        public int Id { get; set; } = 0;
    }
    public class UserDealerGetOutModel : ModelResult
    {
        public UserDealerEditModel UserDealerEdit { get; set; } = new UserDealerEditModel();
    }
    public class UserDealerPutInModel : ModelResult
    {
        public UserDealerEditModel UserDealerEdit { get; set; } = new UserDealerEditModel();
    }
    public class UserDealerPutOutModel : ModelResult
    {
        public UserDealerEditModel UserDealerEdit { get; set; } = new UserDealerEditModel();
    }

}
