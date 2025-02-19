
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.data.Entities
{
    public static class AspNetUsersDataUtil
    {
        public enum AspNetRolesGroup
        {
            SuperUser,

            Admin, // admin interni che assistono tutti
            DealerAdmin, // admin a livello di concessionaria
            FinancialAdvisorAdmin, // admin a livello di studio
            CompanyAdmin, //admin azianda

            User,
            All
        }

        public class AspNetRolesModel()
        {
            public string Name { get; set; } = string.Empty;
            public RoleCode Code { get; set; } = RoleCode.User;
        }

        public static List<AspNetRolesModel> Get_AspNetRoles(AspNetRolesGroup group)
        {
            List<AspNetRolesModel> retVal = new List<AspNetRolesModel>();

            if (group == AspNetRolesGroup.SuperUser || group == AspNetRolesGroup.All)
                retVal.Add(new AspNetRolesModel() { Name = "SuperUser", Code = RoleCode.SuperUser });

            if (group == AspNetRolesGroup.Admin || group == AspNetRolesGroup.All)
                retVal.AddRange(
                                new AspNetRolesModel() { Name = "PowerAdmin", Code = RoleCode.PowerAdmin },
                                new AspNetRolesModel() { Name = "Admin", Code = RoleCode.Admin }
                                );

            if (group == AspNetRolesGroup.DealerAdmin || group == AspNetRolesGroup.All)
                retVal.AddRange(
                                new AspNetRolesModel() { Name = "DealerPowerAdmin", Code = RoleCode.DealerPowerAdmin },
                                new AspNetRolesModel() { Name = "DealerAdmin", Code = RoleCode.DealerAdmin }
                    );

            if (group == AspNetRolesGroup.FinancialAdvisorAdmin || group == AspNetRolesGroup.All)
                retVal.AddRange(
                                new AspNetRolesModel() { Name = "FinancialAdvisorPowerAdmin", Code = RoleCode.FinancialAdvisorPowerAdmin },
                                new AspNetRolesModel() { Name = "FinancialAdvisorAdmin", Code = RoleCode.FinancialAdvisorAdmin }
                    );


            if (group == AspNetRolesGroup.CompanyAdmin || group == AspNetRolesGroup.All)
                retVal.AddRange(
                                new AspNetRolesModel() { Name = "CompanyPowerAdmin", Code = RoleCode.CompanyPowerAdmin },
                                new AspNetRolesModel() { Name = "CompanyAdmin", Code = RoleCode.CompanyAdmin }
                    );

            if (group == AspNetRolesGroup.User || group == AspNetRolesGroup.All)
                retVal.Add(new AspNetRolesModel() { Name = "User", Code = RoleCode.User });

            return retVal;
        }
    }
}
