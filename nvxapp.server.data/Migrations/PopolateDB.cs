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
        }

        public static void PopolateDB_InitDB_UP(MigrationBuilder migrationBuilder)
        {

            if (SharedSchema.CurrentSchema != "public")
                return;


            string pw = "1234";

            //user
            List<Data_AspNetUsers> data_AspNetUsers = new List<Data_AspNetUsers>();
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "SuperUser", Email = "nello68@hotmail.com" });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "PowerAdmin", Email = "nello68@hotmail.com" });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "Admin", Email = "nello68@hotmail.com" });

            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerPowerAdmin", Email = "nello68@hotmail.com" });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "DealerAdmin", Email = "nello68@hotmail.com" });

            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "CompanyPowerAdmin", Email = "nello68@hotmail.com" });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "CompanyAdmin", Email = "nello68@hotmail.com" });

            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "User", Email = "nello68@hotmail.com" });

            List<AspNetRolesModel> roleUser = new List<AspNetRolesModel>();
            roleUser.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.User));

            List<AspNetRolesModel> roleAdmin = new List<AspNetRolesModel>();
            roleAdmin.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.Admin));
            roleAdmin.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.DealerAdmin));
            roleAdmin.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.User));

            List<AspNetRolesModel> roleAll = AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.All);


            List<Data_AspNetRoles> data_AspNetRoles_All = new List<Data_AspNetRoles>();
            foreach (var item in roleAll)
                data_AspNetRoles_All.Add(new Data_AspNetRoles() { Id = Guid.NewGuid().ToString(), Name = item.Name, Code = item.Code });



            ///*TO DB*/

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


            ////user
            
            var hasher = new PasswordHasher<IdentityUser>();
            //var passwordHash = hasher.HashPassword(null, pw);
            var tempUser = new IdentityUser(); // Istanza temporanea di IdentityUser
            var passwordHash = hasher.HashPassword(tempUser, pw);

            foreach (var itemUser in data_AspNetUsers)
            {
                migrationBuilder.InsertData(
                                                table: "AspNetUsers",
                                                //schema: "public",
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


                switch (itemUser.UserName)
                {
                    case "SuperUser":
                        //data_AspNetRoles_to_Scan = data_AspNetRoles_All;
                        data_AspNetRoles_to_Scan = data_AspNetRoles_All.Where(x => x.Name == "SuperUser").ToList();
                        break;

                    case "Admin":
                        //data_AspNetRoles_to_Scan = data_AspNetRoles_All.Where(x => x.Name == "Admin").ToList();

                        var roleAdminKeys =  roleAdmin.Select(x=> x.Name).ToList();
                        

                        data_AspNetRoles_to_Scan = data_AspNetRoles_All.Where(x => roleAdminKeys.Contains(x.Name)  ).ToList();
                        
                        break;

                    default:
                        data_AspNetRoles_to_Scan = data_AspNetRoles_All.Where(x => x.Name == "User").ToList();
                        break;
                }


                foreach (var itemRole in data_AspNetRoles_to_Scan)
                {
                    migrationBuilder.InsertData(
                                     table: "AspNetUserRoles",
                                     //schema: "public",
                                     columns: new[] { "UserId", "RoleId" },
                                     values: new object[] { itemUser.Id, itemRole.Id }
                                     );
                }

            }


        }



    }



}
