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

        public class AspNetRoles_Key_Code()
        {
            public string Key { get; set; }
            public RoleCode Code { get; set; }
        }

        public static List<AspNetRoles_Key_Code> Get_AspNetRoles(AspNetRolesGroup group)
        {
            List<AspNetRoles_Key_Code> retVal = new List<AspNetRoles_Key_Code>();

            if (group == AspNetRolesGroup.SuperUser || group == AspNetRolesGroup.All)
                retVal.Add(new AspNetRoles_Key_Code() { Key = "SuperUser", Code = RoleCode.SuperUser });

            if (group == AspNetRolesGroup.Admin || group == AspNetRolesGroup.All)
                retVal.AddRange(
                                new AspNetRoles_Key_Code() { Key = "PowerAdmin", Code = RoleCode.PowerAdmin },
                                new AspNetRoles_Key_Code() { Key = "Admin", Code = RoleCode.Admin }
                                );

            if (group == AspNetRolesGroup.DealerAdmin || group == AspNetRolesGroup.All)
                retVal.AddRange(
                                new AspNetRoles_Key_Code() { Key = "DealerPowerAdmin", Code = RoleCode.DealerPowerAdmin },
                                new AspNetRoles_Key_Code() { Key = "DealerAdmin", Code = RoleCode.DealerAdmin }
                    );

            if (group == AspNetRolesGroup.CompanyAdmin || group == AspNetRolesGroup.All)
                retVal.AddRange(
                                new AspNetRoles_Key_Code() { Key = "CompanyPowerAdmin", Code = RoleCode.CompanyPowerAdmin },
                                new AspNetRoles_Key_Code() { Key = "CompanyAdmin", Code = RoleCode.CompanyAdmin }
                    );

            if (group == AspNetRolesGroup.User || group == AspNetRolesGroup.All)
                retVal.Add(new AspNetRoles_Key_Code() { Key = "User", Code = RoleCode.User });

            return retVal;
        }
    }
}
