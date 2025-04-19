using Microsoft.AspNetCore.Identity;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Parameter.Models
{
    

    public class RolesListInModel
    {
    }
    public class RolesListOutModel : ModelResult
    {
        public List<ApplicationRole> Roles { get; set; }

        public RolesListOutModel()
        {
            Roles = new List<ApplicationRole>();
        }
    }

}
