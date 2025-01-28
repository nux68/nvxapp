using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Service.MyTableService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.Account.Models
{
    public class UserRolesInModel
    {

    }

    public class UserRolesOutModel : ModelResult
    {
        
        public List<string> Roles { get; set; }

        public UserRolesOutModel()
        {
             Roles = new List<string>();
        }
    }

   
}
