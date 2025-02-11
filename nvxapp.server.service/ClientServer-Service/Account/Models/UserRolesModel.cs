using nvxapp.server.data.Entities;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Service.MyTableService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.ClientServer_Service.Account.Models
{
    public class UserRolesInModel
    {

    }

    public class UserRolesOutModel : ModelResult
    {
        
        public List<AspNetRolesModel> Roles { get; set; }

        public UserRolesOutModel()
        {
             Roles = new List<AspNetRolesModel>();
        }
    }




}
