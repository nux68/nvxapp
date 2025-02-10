using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities
{
    public static class AspNetUsersDataUtil
    {
        public enum AspNetRolesGroup
        {
            SuperUser,

            Admin, // admin interni che assistono tutti
            DealerAdmin, // admin a livello di concessionaria
            CompanyAdmin, //admin azianda

            User,
            All
        }

        public static List<string> Get_AspNetRoles(AspNetRolesGroup group)
        {
            List<string> retVal = new List<string>();

            if (group == AspNetRolesGroup.SuperUser || group == AspNetRolesGroup.All)
                retVal.Add("SuperUser");

            if (group == AspNetRolesGroup.Admin || group == AspNetRolesGroup.All)
                retVal.AddRange("PowerAdmin", "Admin");

            if (group == AspNetRolesGroup.DealerAdmin || group == AspNetRolesGroup.All)
                retVal.AddRange("DealerPowerAdmin", "DealerAdmin");

            if (group == AspNetRolesGroup.CompanyAdmin || group == AspNetRolesGroup.All)
                retVal.AddRange("CompanyPowerAdmin", "CompanyAdmin");

            if (group == AspNetRolesGroup.User || group == AspNetRolesGroup.All)
                retVal.Add("User");

            return retVal;
        }
    }
}
