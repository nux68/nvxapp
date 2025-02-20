using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Public;
using System.Data;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.data.Migrations
{
    public class PopolateDB
    {

        protected readonly IAspNetUsersRepository _aspNetUsersRepository;


        public PopolateDB(IAspNetUsersRepository aspNetUsersRepository)
        {
            _aspNetUsersRepository = aspNetUsersRepository;
        }


        class Data_AspNetRoles
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public RoleCode Code { get; set; } = RoleCode.User;
        }

        class Data_AspNetUsers
        {
            public string Id { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public RoleCode? Code { get; set; }
        }

        public static void PopolateDB_InitDB_UP(MigrationBuilder migrationBuilder)
        {

            if (SharedSchema.CurrentSchema != "public")
                return;


            ////////////string pw = "1234";

            //////////////user
            ////////////List<Data_AspNetUsers> data_AspNetUsers = new List<Data_AspNetUsers>();
            ////////////data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "SuperUser", Email = "nello68@hotmail.com" });
            ////////////data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "PowerAdmin", Email = "nello68@hotmail.com" });
            ////////////data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "Admin", Email = "nello68@hotmail.com" });

            ////////////data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerPowerAdmin", Email = "nello68@hotmail.com" });
            ////////////data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerAdmin", Email = "nello68@hotmail.com" });

            ////////////data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "FinancialAdvisorPowerAdmin", Email = "nello68@hotmail.com" });
            ////////////data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "FinancialAdvisorAdmin", Email = "nello68@hotmail.com" });

            ////////////data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "CompanyPowerAdmin", Email = "nello68@hotmail.com" });
            ////////////data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "CompanyAdmin", Email = "nello68@hotmail.com" });

            ////////////data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "User", Email = "nello68@hotmail.com" });

            //////////////List<AspNetRolesModel> roleUser = new List<AspNetRolesModel>();
            //////////////roleUser.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.User));

            //////////////List<AspNetRolesModel> roleAdmin = new List<AspNetRolesModel>();
            //////////////roleAdmin.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.Admin));
            //////////////roleAdmin.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.DealerAdmin));
            //////////////roleAdmin.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.User));

            ////////////List<AspNetRolesModel> roleAll = AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.All);


            ////////////List<Data_AspNetRoles> data_AspNetRoles_All = new List<Data_AspNetRoles>();
            ////////////foreach (var item in roleAll)
            ////////////    data_AspNetRoles_All.Add(new Data_AspNetRoles() { Id = Guid.NewGuid().ToString(), Name = item.Name, Code = item.Code });



            ///////////////*TO DB*/

            //////////////AspNetRoles
            ////////////List<Data_AspNetRoles> data_AspNetRoles_to_Scan = data_AspNetRoles_All;

            ////////////foreach (var item in data_AspNetRoles_to_Scan)
            ////////////{
            ////////////    migrationBuilder.InsertData(
            ////////////                  table: "AspNetRoles",
            ////////////                  columns: new[] { "Id", "Code", "Name", "NormalizedName" },
            ////////////                  values: new object[] { item.Id, (int)item.Code, item.Name, item.Name.ToUpper() }
            ////////////                  );
            ////////////}


            ////////////////user

            ////////////var hasher = new PasswordHasher<IdentityUser>();
            //////////////var passwordHash = hasher.HashPassword(null, pw);
            ////////////var tempUser = new IdentityUser(); // Istanza temporanea di IdentityUser
            ////////////var passwordHash = hasher.HashPassword(tempUser, pw);

            ////////////foreach (var itemUser in data_AspNetUsers)
            ////////////{
            ////////////    migrationBuilder.InsertData(
            ////////////                                    table: "AspNetUsers",
            ////////////                                    //schema: "public",
            ////////////                                    columns: new[]
            ////////////                                    {
            ////////////                                            "Id",
            ////////////                                            "UserName",
            ////////////                                            "NormalizedUserName",
            ////////////                                            "Email",
            ////////////                                            "NormalizedEmail",
            ////////////                                            "EmailConfirmed",
            ////////////                                            "PasswordHash",
            ////////////                                            "SecurityStamp",
            ////////////                                            "ConcurrencyStamp",
            ////////////                                            "LockoutEnabled",
            ////////////                                            "TwoFactorEnabled",
            ////////////                                            "PhoneNumberConfirmed",
            ////////////                                            "AccessFailedCount"
            ////////////                                    },
            ////////////                                    values: new object[]
            ////////////                                    {
            ////////////                                            itemUser.Id,
            ////////////                                            itemUser.UserName,
            ////////////                                            itemUser.UserName.ToUpper(),
            ////////////                                            itemUser.Email,
            ////////////                                            itemUser.Email.ToUpper(),
            ////////////                                            true,
            ////////////                                            passwordHash,
            ////////////                                            Guid.NewGuid().ToString(),
            ////////////                                            Guid.NewGuid().ToString(),
            ////////////                                            false,
            ////////////                                            false,
            ////////////                                            false,
            ////////////                                            0
            ////////////                                    }
            ////////////    );




            ////////////    var curr_AspNetRoles = data_AspNetRoles_All.Where(x => x.Name == itemUser.UserName).FirstOrDefault();
            ////////////    if (curr_AspNetRoles != null)
            ////////////    {
            ////////////        migrationBuilder.InsertData(
            ////////////                        table: "AspNetUserRoles",
            ////////////                        columns: new[] { "UserId", "RoleId" },
            ////////////                        values: new object[] { itemUser.Id, curr_AspNetRoles.Id }
            ////////////                        );

            ////////////        //per questi ruoli creo i dati default 
            ////////////        switch(curr_AspNetRoles.Code)
            ////////////        {
            ////////////            case RoleCode.DealerPowerAdmin:

            ////////////                migrationBuilder.InsertData(
            ////////////                        table: "Dealer",
            ////////////                        columns: new[] { "Descrizione" },
            ////////////                        values: new object[] { "Concessionario Default" }
            ////////////                        );

            ////////////                migrationBuilder.InsertData(
            ////////////                        table: "UserDealer",
            ////////////                        columns: new[] { "IdDealer", "IdAspNetUsers" },
            ////////////                        values: new object[] { 1, itemUser .Id}
            ////////////                        );



            ////////////                break;
            ////////////            case RoleCode.FinancialAdvisorPowerAdmin:

            ////////////                migrationBuilder.InsertData(
            ////////////                        table: "FinancialAdvisor",
            ////////////                        columns: new[] { "IdDealer", "Descrizione" },
            ////////////                        values: new object[] { 1,"Studio Default" }
            ////////////                        );

            ////////////                migrationBuilder.InsertData(
            ////////////                       table: "UserFinancialAdvisor",
            ////////////                       columns: new[] { "IdFinancialAdvisor", "IdAspNetUsers" },
            ////////////                       values: new object[] { 1, itemUser.Id }
            ////////////                       );

            ////////////                break;
            ////////////            case RoleCode.CompanyPowerAdmin:

            ////////////                migrationBuilder.InsertData(
            ////////////                      table: "Company",
            ////////////                      columns: new[] { "IdFinancialAdvisor","Descrizione", "Schema" },
            ////////////                      values: new object[] { 1,"Azienda Default", "schema_default" }
            ////////////                      );

            ////////////                migrationBuilder.InsertData(
            ////////////                     table: "UserCompany",
            ////////////                     columns: new[] { "IdCompany", "IdAspNetUsers" },
            ////////////                     values: new object[] { 1, itemUser.Id }
            ////////////                     );


            ////////////                break;
            ////////////            case RoleCode.User:
            ////////////                migrationBuilder.InsertData(
            ////////////                     table: "UserCompany",
            ////////////                     columns: new[] { "IdCompany", "IdAspNetUsers" },
            ////////////                     values: new object[] { 1, itemUser.Id }
            ////////////                     );


            ////////////                break;

            ////////////        }

            ////////////    }


            ////////////}

            List<AspNetRolesModel> roleAll = AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.All);

            List<Data_AspNetRoles> data_AspNetRoles_All = new List<Data_AspNetRoles>();
            foreach (var item in roleAll)
                data_AspNetRoles_All.Add(new Data_AspNetRoles() { Id = Guid.NewGuid().ToString(), Name = item.Name, Code = item.Code });

            //TO DB

            //AspNetRoles
            List<Data_AspNetRoles> data_AspNetRoles_to_Scan = data_AspNetRoles_All;

            foreach (var item in data_AspNetRoles_to_Scan)
            {
                migrationBuilder.InsertData(
                              table: "AspNetRoles",
                              columns: new[] { "Id", "Code", "Name", "NormalizedName" },
                              values: new object[] { item.Id, (int)item.Code, item.Name, item.Name.ToUpper() }
                              );
            }

            List<Data_AspNetUsers> data_AspNetUsers;
            data_AspNetUsers = PopolateDB_Init_Base();

            PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

            PopolateDB_Fill_all(migrationBuilder, data_AspNetRoles_All);

            //for (int idxDealer = 1; idxDealer <= 2; idxDealer++)
            //{
            //    data_AspNetUsers = PopolateDB_Init_User(idxDealer);
            //    PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers,idxDealer);
            //}



        }

        private static List<Data_AspNetUsers> PopolateDB_Init_Base()
        {
            List<Data_AspNetUsers> data_AspNetUsers = new List<Data_AspNetUsers>();
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "SuperUser", Email = "nello68@hotmail.com", Code = RoleCode.SuperUser });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "PowerAdmin", Email = "nello68@hotmail.com", Code = RoleCode.PowerAdmin });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "Admin", Email = "nello68@hotmail.com", Code = RoleCode.Admin });

            return data_AspNetUsers;
        }

        private static void PopolateDB_Fill_all(MigrationBuilder migrationBuilder, List<Data_AspNetRoles> data_AspNetRoles_All)
        {
            List<Data_AspNetUsers> data_AspNetUsers = new List<Data_AspNetUsers>();

            int idxFinancialAdvisor = 0;
            int idxCompany = 0;
            int idxUser = 0;
            for (int idxDealer = 1; idxDealer <= 2; idxDealer++)
            {
                string key_Dealer = "_" + idxDealer.ToString();
                data_AspNetUsers = new List<Data_AspNetUsers>();

                data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerPowerAdmin" + key_Dealer, Email = "nello68@hotmail.com", Code = RoleCode.DealerPowerAdmin });
                data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerAdmin" + key_Dealer, Email = "nello68@hotmail.com", Code = RoleCode.DealerAdmin });
                PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

                migrationBuilder.InsertData(
                        table: "Dealer",
                        columns: new[] { "Descrizione" },
                        values: new object[] { "Concessionario " + key_Dealer }
                        );

                migrationBuilder.InsertData(
                        table: "UserDealer",
                        columns: new[] { "IdDealer", "IdAspNetUsers" },
                        values: new object[] { idxDealer, data_AspNetUsers[0].Id }
                        );

                for (int idxFinancial = 1; idxFinancial <= 10; idxFinancial++)
                {
                    string key_FinancialAdvisor = key_Dealer + "_" + idxFinancial.ToString();
                    idxFinancialAdvisor++;

                    data_AspNetUsers = new List<Data_AspNetUsers>();
                    data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "FinancialAdvisorPowerAdmin" + key_FinancialAdvisor, Email = "nello68@hotmail.com", Code = RoleCode.FinancialAdvisorPowerAdmin });
                    data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "FinancialAdvisorAdmin" + key_FinancialAdvisor, Email = "nello68@hotmail.com", Code = RoleCode.FinancialAdvisorAdmin });
                    PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

                    migrationBuilder.InsertData(
                                    table: "FinancialAdvisor",
                                    columns: new[] { "IdDealer", "Descrizione" },
                                    values: new object[] { idxDealer, "Studio " + key_FinancialAdvisor }
                                    );

                    migrationBuilder.InsertData(
                           table: "UserFinancialAdvisor",
                           columns: new[] { "IdFinancialAdvisor", "IdAspNetUsers" },
                           values: new object[] { idxFinancialAdvisor, data_AspNetUsers[0].Id }
                           );


                    for (int idxComp = 1; idxComp <= 10; idxComp++)
                    {
                        string key_Company = key_FinancialAdvisor + "_" + idxComp.ToString();
                        idxCompany++;

                        data_AspNetUsers = new List<Data_AspNetUsers>();
                        data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "CompanyPowerAdmin" + key_Company, Email = "nello68@hotmail.com", Code = RoleCode.CompanyPowerAdmin });
                        data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "CompanyAdmin" + key_Company, Email = "nello68@hotmail.com", Code = RoleCode.CompanyAdmin });
                        PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

                        migrationBuilder.InsertData(
                                                      table: "Company",
                                                      columns: new[] { "IdFinancialAdvisor", "Descrizione", "Schema" },
                                                      values: new object[] { idxFinancialAdvisor, "Azienda " + key_Company, "schema_" + key_Company }
                                                      );

                        migrationBuilder.InsertData(
                             table: "UserCompany",
                             columns: new[] { "IdCompany", "IdAspNetUsers" },
                             values: new object[] { idxCompany, data_AspNetUsers[0].Id }
                             );

                        for (int idxUs = 1; idxUs <= 10; idxUs++)
                        {
                            idxUser++;
                            string key_User = key_Company + "_" + idxUs.ToString();
                            data_AspNetUsers = new List<Data_AspNetUsers>();
                            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "User"+ key_User, Email = "nello68@hotmail.com", Code = RoleCode.User });
                            PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

                            migrationBuilder.InsertData(
                                                         table: "UserCompany",
                                                         columns: new[] { "IdCompany", "IdAspNetUsers" },
                                                         values: new object[] { idxCompany, data_AspNetUsers[0].Id }
                                                         );
                        }

                    }


                }

            }


        }



        //private static List<Data_AspNetUsers> PopolateDB_Init_User(int CodDealer)
        //{
        //    List<Data_AspNetUsers> data_AspNetUsers = new List<Data_AspNetUsers>();

        //    string key_Dealer = "_" + CodDealer.ToString();

        //    data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerPowerAdmin" + key_Dealer, Email = "nello68@hotmail.com", Code = RoleCode.DealerPowerAdmin });
        //    data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerAdmin" + key_Dealer, Email = "nello68@hotmail.com", Code = RoleCode.DealerAdmin });

        //    data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "FinancialAdvisorPowerAdmin", Email = "nello68@hotmail.com", Code = RoleCode.FinancialAdvisorPowerAdmin });
        //    data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "FinancialAdvisorAdmin", Email = "nello68@hotmail.com", Code = RoleCode.FinancialAdvisorAdmin });

        //    //data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "CompanyPowerAdmin", Email = "nello68@hotmail.com", Code = RoleCode.CompanyPowerAdmin });
        //    //data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "CompanyAdmin", Email = "nello68@hotmail.com", Code = RoleCode.CompanyAdmin });

        //    //data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "User", Email = "nello68@hotmail.com", Code = RoleCode.User });

        //    return data_AspNetUsers;
        //}


        private static void PopolateDB_Save_User(MigrationBuilder migrationBuilder,
                                                 List<Data_AspNetRoles> data_AspNetRoles_All,
                                                 List<Data_AspNetUsers> data_AspNetUsers)
        {
            string pw = "1234";

            var hasher = new PasswordHasher<IdentityUser>();
            //var passwordHash = hasher.HashPassword(null, pw);
            var tempUser = new IdentityUser(); // Istanza temporanea di IdentityUser
            var passwordHash = hasher.HashPassword(tempUser, pw);

            foreach (var itemUser in data_AspNetUsers)
            {
                migrationBuilder.InsertData(
                                                table: "AspNetUsers",
                                                columns: new[]
                                                {
                                                        "Id",
                                                        "UserName",
                                                        "NormalizedUserName",
                                                        "Email",
                                                        "NormalizedEmail",
                                                        "EmailConfirmed",
                                                        "PasswordHash",
                                                        "SecurityStamp",
                                                        "ConcurrencyStamp",
                                                        "LockoutEnabled",
                                                        "TwoFactorEnabled",
                                                        "PhoneNumberConfirmed",
                                                        "AccessFailedCount"
                                                },
                                                values: new object[]
                                                {
                                                        itemUser.Id,
                                                        itemUser.UserName,
                                                        itemUser.UserName.ToUpper(),
                                                        itemUser.Email,
                                                        itemUser.Email.ToUpper(),
                                                        true,
                                                        passwordHash,
                                                        Guid.NewGuid().ToString(),
                                                        Guid.NewGuid().ToString(),
                                                        false,
                                                        false,
                                                        false,
                                                        0
                                                }
                );

                var curr_AspNetRoles = data_AspNetRoles_All.Where(x => x.Code == itemUser.Code).FirstOrDefault();
                if (curr_AspNetRoles != null)
                {
                    migrationBuilder.InsertData(
                                    table: "AspNetUserRoles",
                                    columns: new[] { "UserId", "RoleId" },
                                    values: new object[] { itemUser.Id, curr_AspNetRoles.Id }
                                    );
                }


            }

        }


        //private static void PopolateDB_Save_User(MigrationBuilder migrationBuilder, 
        //                                         List<Data_AspNetRoles> data_AspNetRoles_All, 
        //                                         List<Data_AspNetUsers> data_AspNetUsers,
        //                                         int idxDealer)
        //{
        //    string pw = "1234";

        //    var hasher = new PasswordHasher<IdentityUser>();
        //    //var passwordHash = hasher.HashPassword(null, pw);
        //    var tempUser = new IdentityUser(); // Istanza temporanea di IdentityUser
        //    var passwordHash = hasher.HashPassword(tempUser, pw);

        //    foreach (var itemUser in data_AspNetUsers)
        //    {
        //        migrationBuilder.InsertData(
        //                                        table: "AspNetUsers",
        //                                        //schema: "public",
        //                                        columns: new[]
        //                                        {
        //                                                "Id",
        //                                                "UserName",
        //                                                "NormalizedUserName",
        //                                                "Email",
        //                                                "NormalizedEmail",
        //                                                "EmailConfirmed",
        //                                                "PasswordHash",
        //                                                "SecurityStamp",
        //                                                "ConcurrencyStamp",
        //                                                "LockoutEnabled",
        //                                                "TwoFactorEnabled",
        //                                                "PhoneNumberConfirmed",
        //                                                "AccessFailedCount"
        //                                        },
        //                                        values: new object[]
        //                                        {
        //                                                itemUser.Id,
        //                                                itemUser.UserName,
        //                                                itemUser.UserName.ToUpper(),
        //                                                itemUser.Email,
        //                                                itemUser.Email.ToUpper(),
        //                                                true,
        //                                                passwordHash,
        //                                                Guid.NewGuid().ToString(),
        //                                                Guid.NewGuid().ToString(),
        //                                                false,
        //                                                false,
        //                                                false,
        //                                                0
        //                                        }
        //        );


        //        var sufixDealer="";
        //        var sufixFinancialAdvisor = "";

        //        var curr_AspNetRoles = data_AspNetRoles_All.Where(x => x.Code == itemUser.Code).FirstOrDefault();
        //        if (curr_AspNetRoles != null)
        //        {
        //            migrationBuilder.InsertData(
        //                            table: "AspNetUserRoles",
        //                            columns: new[] { "UserId", "RoleId" },
        //                            values: new object[] { itemUser.Id, curr_AspNetRoles.Id }
        //                            );

        //            //per questi ruoli creo i dati default 
        //            switch (curr_AspNetRoles.Code)
        //            {
        //                case RoleCode.DealerPowerAdmin:

        //                    var sufix = itemUser.UserName.Split("_");
        //                    sufixDealer = "_" + sufix[1];
        //                    migrationBuilder.InsertData(
        //                            table: "Dealer",
        //                            columns: new[] { "Descrizione" },
        //                            values: new object[] { "Concessionario " + sufixDealer }
        //                            );

        //                    migrationBuilder.InsertData(
        //                            table: "UserDealer",
        //                            columns: new[] { "IdDealer", "IdAspNetUsers" },
        //                            values: new object[] { idxDealer, itemUser.Id }
        //                            );



        //                    break;

        //                case RoleCode.FinancialAdvisorPowerAdmin:

        //                    int start = ((idxDealer - 1) * 10);
        //                    for (int i = 1;i<=10;i++)
        //                    {
        //                        sufixFinancialAdvisor = sufixDealer + "_" + i.ToString();

        //                        int curr_id = start + 1;
        //                        migrationBuilder.InsertData(
        //                            table: "FinancialAdvisor",
        //                            columns: new[] { "IdDealer", "Descrizione" },
        //                            values: new object[] { curr_id, "Studio " + sufixFinancialAdvisor }
        //                            );

        //                        //migrationBuilder.InsertData(
        //                        //       table: "UserFinancialAdvisor",
        //                        //       columns: new[] { "IdFinancialAdvisor", "IdAspNetUsers" },
        //                        //       values: new object[] { curr_id, itemUser.Id }
        //                        //       );
        //                    }



        //                        break;

        //                    //case RoleCode.CompanyPowerAdmin:

        //                    //    migrationBuilder.InsertData(
        //                    //          table: "Company",
        //                    //          columns: new[] { "IdFinancialAdvisor", "Descrizione", "Schema" },
        //                    //          values: new object[] { 1, "Azienda Default", "schema_default" }
        //                    //          );

        //                    //    migrationBuilder.InsertData(
        //                    //         table: "UserCompany",
        //                    //         columns: new[] { "IdCompany", "IdAspNetUsers" },
        //                    //         values: new object[] { 1, itemUser.Id }
        //                    //         );


        //                    //    break;
        //                    //case RoleCode.User:
        //                    //    migrationBuilder.InsertData(
        //                    //         table: "UserCompany",
        //                    //         columns: new[] { "IdCompany", "IdAspNetUsers" },
        //                    //         values: new object[] { 1, itemUser.Id }
        //                    //         );


        //                    //    break;

        //            }

        //        }


        //    }

        //}

    }



}
