using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities.Public
{
    public class ApplicationRole : IdentityRole<string>
    {

        public RoleCode Code { get; set; }

    }

    public enum RoleCode
    {
        User = 0,

        CompanyAdmin = 10,
        CompanyPowerAdmin = 11,

        FinancialAdvisorAdmin = 100,
        FinancialAdvisorPowerAdmin = 101,

        DealerAdmin = 1000,
        DealerPowerAdmin = 1001,

        Admin = 10000,
        PowerAdmin = 10001,

        SuperUser = 100000,

    }

}
