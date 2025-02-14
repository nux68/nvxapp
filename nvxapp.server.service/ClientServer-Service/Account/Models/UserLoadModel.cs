using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.ClientServer_Service.Account.Models
{
    public class UserLoadInModel
    {
        public string? Id { get; set; }
    }

    public class UserLoadOutModel : ModelResult
    {
        public UserDataModel UserData { get; set; }
        
        public UserLoadOutModel(){
            UserData = new UserDataModel();
        }
    }

    public class UserDataModel 
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public List<AspNetRolesModel> Roles { get; set; }

        public UserDataModel()
        {
            Roles = new List<AspNetRolesModel>();
        }
    }

}
