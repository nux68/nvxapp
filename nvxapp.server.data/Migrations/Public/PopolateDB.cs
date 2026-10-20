using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using System.Data;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.data.Migrations.Public
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



        }
        private static List<Data_AspNetUsers> PopolateDB_Init_Base()
        {
            List<Data_AspNetUsers> data_AspNetUsers = new List<Data_AspNetUsers>();
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "SuperUser", Email = "myMail@myMail.com", Code = RoleCode.SuperUser });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "PowerAdmin", Email = "myMail@myMail.com", Code = RoleCode.PowerAdmin });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "Admin", Email = "myMail@myMail.com", Code = RoleCode.Admin });

            return data_AspNetUsers;
        }
        private static void PopolateDB_Fill_all(MigrationBuilder migrationBuilder, List<Data_AspNetRoles> data_AspNetRoles_All)
        {
            List<Data_AspNetUsers> data_AspNetUsers = new List<Data_AspNetUsers>();

            int idxFinancialAdvisor = 0;
            int idxCompany = 0;
            int idxUser = 0;
            int numDealer = 2;
            for (int idxDealer = 1; idxDealer <= numDealer; idxDealer++)
            {
                string key_Dealer = "_" + idxDealer.ToString();
                data_AspNetUsers = new List<Data_AspNetUsers>();

                data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerPowerAdmin" + key_Dealer, Email = "myMail@myMail.com", Code = RoleCode.DealerPowerAdmin });
                data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerAdmin" + key_Dealer, Email = "myMail@myMail.com", Code = RoleCode.DealerAdmin });
                PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

                migrationBuilder.InsertData(
                        table: "Dealer",
                        columns: new[] { "Descrizione" },
                        values: new object[] { "Concessionario " + key_Dealer }
                        );
                //DealerPowerAdmin
                migrationBuilder.InsertData(
                        table: "UserDealer",
                        columns: new[] { "IdDealer", "IdAspNetUsers", "MainUser", "Disabled" },
                        values: new object[] { idxDealer, data_AspNetUsers[0].Id ,true, false }
                        );
                //DealerAdmin
                migrationBuilder.InsertData(
                        table: "UserDealer",
                        columns: new[] { "IdDealer", "IdAspNetUsers", "MainUser", "Disabled" },
                        values: new object[] { idxDealer, data_AspNetUsers[1].Id ,true, false }
                        );


                // 3 studi per dealer, 5 aziende per studio, 5 dipendenti per azienda -> 6 studi, 30 aziende, 150 dipendenti
                for (int idxFinancial = 1; idxFinancial <= 3; idxFinancial++)
                {
                    string key_FinancialAdvisor = key_Dealer + "_" + idxFinancial.ToString();
                    idxFinancialAdvisor++;

                    data_AspNetUsers = new List<Data_AspNetUsers>();
                    data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "FinancialAdvisorPowerAdmin" + key_FinancialAdvisor, Email = "myMail@myMail.com", Code = RoleCode.FinancialAdvisorPowerAdmin });
                    data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "FinancialAdvisorAdmin" + key_FinancialAdvisor, Email = "myMail@myMail.com", Code = RoleCode.FinancialAdvisorAdmin });
                    PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

                    migrationBuilder.InsertData(
                                    table: "FinancialAdvisor",
                                    columns: new[] { "IdDealer", "Descrizione" },
                                    values: new object[] { idxDealer, "Studio " + key_FinancialAdvisor }
                                    );
                    //FinancialAdvisorPowerAdmin
                    migrationBuilder.InsertData(
                           table: "UserFinancialAdvisor",
                           columns: new[] { "IdFinancialAdvisor", "IdAspNetUsers" , "MainUser", "Disabled" },
                           values: new object[] { idxFinancialAdvisor, data_AspNetUsers[0].Id ,true, false }
                           );
                    //FinancialAdvisorAdmin
                    migrationBuilder.InsertData(
                           table: "UserFinancialAdvisor",
                           columns: new[] { "IdFinancialAdvisor", "IdAspNetUsers", "MainUser", "Disabled" },
                           values: new object[] { idxFinancialAdvisor, data_AspNetUsers[1].Id ,true, false }
                           );


                    for (int idxComp = 1; idxComp <= 5; idxComp++)
                    {
                        string key_Company = key_FinancialAdvisor + "_" + idxComp.ToString();
                        idxCompany++;

                        data_AspNetUsers = new List<Data_AspNetUsers>();
                        data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "CompanyPowerAdmin" + key_Company, Email = "myMail@myMail.com", Code = RoleCode.CompanyPowerAdmin });
                        data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "CompanyAdmin" + key_Company, Email = "myMail@myMail.com", Code = RoleCode.CompanyAdmin });
                        PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

                        migrationBuilder.InsertData(
                                                      table: "Company",
                                                      columns: new[] { "IdFinancialAdvisor", "Descrizione" },
                                                      values: new object[] { idxFinancialAdvisor, "Azienda " + key_Company }
                                                      );

                        //CompanyPowerAdmin
                        migrationBuilder.InsertData(
                             table: "UserCompany",
                             columns: new[] { "IdCompany", "IdAspNetUsers" , "MainUser", "Disabled" },
                             values: new object[] { idxCompany, data_AspNetUsers[0].Id,true, false }
                             );
                        //CompanyAdmin
                        migrationBuilder.InsertData(
                             table: "UserCompany",
                             columns: new[] { "IdCompany", "IdAspNetUsers" , "MainUser", "Disabled" },
                             values: new object[] { idxCompany, data_AspNetUsers[1].Id ,true, false }
                             );

                        for (int idxUs = 1; idxUs <= 5; idxUs++)
                        {
                            idxUser++;
                            string key_User = key_Company + "_" + idxUs.ToString();
                            data_AspNetUsers = new List<Data_AspNetUsers>();
                            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "User"+ key_User, Email = "myMail@myMail.com", Code = RoleCode.User });
                            PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

                            migrationBuilder.InsertData(
                                                         table: "UserCompany",
                                                         columns: new[] { "IdCompany", "IdAspNetUsers", "MainUser", "Disabled" },
                                                         values: new object[] { idxCompany, data_AspNetUsers[0].Id,false, false }
                                                         );
                        }

                    }


                }

            }

            // DEMO: un dealer e uno studio (FinancialAdvisor) con chiave DEMO, inseriti in coda per non cambiare gli Id esistenti.
            // Come gli altri hanno i loro utenti principali: le liste Centri e Studi partono dagli utenti PowerAdmin principali.
            string key_Demo = "_DEMO";
            int idDealerDemo = numDealer + 1;
            int idFinancialAdvisorDemo = idxFinancialAdvisor + 1;

            data_AspNetUsers = new List<Data_AspNetUsers>();
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerPowerAdmin" + key_Demo, Email = "myMail@myMail.com", Code = RoleCode.DealerPowerAdmin });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerAdmin" + key_Demo, Email = "myMail@myMail.com", Code = RoleCode.DealerAdmin });
            PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

            migrationBuilder.InsertData(
                    table: "Dealer",
                    columns: new[] { "Descrizione" },
                    values: new object[] { "DEMO" }
                    );
            //DealerPowerAdmin
            migrationBuilder.InsertData(
                    table: "UserDealer",
                    columns: new[] { "IdDealer", "IdAspNetUsers", "MainUser", "Disabled" },
                    values: new object[] { idDealerDemo, data_AspNetUsers[0].Id, true, false }
                    );
            //DealerAdmin
            migrationBuilder.InsertData(
                    table: "UserDealer",
                    columns: new[] { "IdDealer", "IdAspNetUsers", "MainUser", "Disabled" },
                    values: new object[] { idDealerDemo, data_AspNetUsers[1].Id, true, false }
                    );

            data_AspNetUsers = new List<Data_AspNetUsers>();
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "FinancialAdvisorPowerAdmin" + key_Demo, Email = "myMail@myMail.com", Code = RoleCode.FinancialAdvisorPowerAdmin });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "FinancialAdvisorAdmin" + key_Demo, Email = "myMail@myMail.com", Code = RoleCode.FinancialAdvisorAdmin });
            PopolateDB_Save_User(migrationBuilder, data_AspNetRoles_All, data_AspNetUsers);

            migrationBuilder.InsertData(
                    table: "FinancialAdvisor",
                    columns: new[] { "IdDealer", "Descrizione" },
                    values: new object[] { idDealerDemo, "DEMO" }
                    );
            //FinancialAdvisorPowerAdmin
            migrationBuilder.InsertData(
                    table: "UserFinancialAdvisor",
                    columns: new[] { "IdFinancialAdvisor", "IdAspNetUsers", "MainUser", "Disabled" },
                    values: new object[] { idFinancialAdvisorDemo, data_AspNetUsers[0].Id, true, false }
                    );
            //FinancialAdvisorAdmin
            migrationBuilder.InsertData(
                    table: "UserFinancialAdvisor",
                    columns: new[] { "IdFinancialAdvisor", "IdAspNetUsers", "MainUser", "Disabled" },
                    values: new object[] { idFinancialAdvisorDemo, data_AspNetUsers[1].Id, true, false }
                    );

        }
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


    }



}
