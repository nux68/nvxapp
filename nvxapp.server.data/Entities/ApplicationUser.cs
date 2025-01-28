using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities
{
    public class ApplicationUser : IdentityUser, IDataChangeStatEntity
    {
        public DateTime? ModifiedDate { get; set; }
        public DateTime? CreationDate { get; set; }
        [MaxLength(256)]
        public string? ChangeUser { get; set; }
    }


    public static class AspNetUsersDataUtil
    {
        public enum AspNetRolesGroup
        {
            SuperUser,
            Admin,
            DomainAdmin,
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

            if (group == AspNetRolesGroup.DomainAdmin || group == AspNetRolesGroup.All)
                retVal.AddRange("DomainPowerAdmin", "DomainAdmin");

            if (group == AspNetRolesGroup.User || group == AspNetRolesGroup.All)
                retVal.Add("User");

            return retVal;
        }
    }


}
