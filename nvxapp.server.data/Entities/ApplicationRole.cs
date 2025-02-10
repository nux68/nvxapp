using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities
{
    public class ApplicationRole : IdentityRole<string>
    {

        public RoleCode Code { get; set; }  

    }

    public enum RoleCode
    {
        User =0,

        CompanyAdmin =10,
        CompanyAdminPowerUser = 11,

        DealerAdmin = 100,
        DealerAdminPowerUser = 101,

        Admin = 1000,
        AdminPowerUser = 1001,

        SuperUser = 10000,

    }

}
