using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Repositories;
using System.Data;

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
        }

        class Data_AspNetUsers
        {
            public string Id { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public static void PopolateDB_InitDB_UP(MigrationBuilder migrationBuilder)
        {
            string pw = "1234";

            //user
            List<Data_AspNetUsers> data_AspNetUsers = new List<Data_AspNetUsers>();
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "NVX", Email = "nello68@hotmail.com" });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "NVX2", Email = "nello68@hotmail.com" });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "NVX3", Email = "nello68@hotmail.com" });


            List<string> roleUser = new List<string>();
            roleUser.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.User));

            List<string> roleAdmin = new List<string>();
            roleAdmin.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.Admin));
            roleAdmin.AddRange(AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.User));

            List<string> roleAll = AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.All);


            List<Data_AspNetRoles> data_AspNetRoles_All = new List<Data_AspNetRoles>();
            foreach (var item in roleAll)
                data_AspNetRoles_All.Add(new Data_AspNetRoles() { Id = Guid.NewGuid().ToString(), Name = item });
            


            ///*TO DB*/

            //AspNetRoles
            List<Data_AspNetRoles> data_AspNetRoles_to_Scan = data_AspNetRoles_All.Where(x => roleAll.Contains(x.Name)).ToList();

            foreach (var item in data_AspNetRoles_to_Scan)
            {
                migrationBuilder.InsertData(
                              table: "AspNetRoles",
                              columns: new[] { "Id", "Name", "NormalizedName" },
                              values: new object[] { item.Id, item.Name, item.Name.ToUpper() }
                              );
            }


            //user
            var userId = Guid.NewGuid().ToString();
            var hasher = new PasswordHasher<IdentityUser>();
            var passwordHash = hasher.HashPassword(null, pw);

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


                switch (itemUser.UserName)
                {
                    case "NVX":
                        data_AspNetRoles_to_Scan = data_AspNetRoles_All;
                        break;

                    case "NVX3":
                        data_AspNetRoles_to_Scan = data_AspNetRoles_All.Where(x => roleAdmin.Contains(x.Name)).ToList();
                        break;

                    default:
                        data_AspNetRoles_to_Scan = data_AspNetRoles_All.Where(x => roleUser.Contains(x.Name)).ToList();
                        break;
                }
                

                foreach (var itemRole in data_AspNetRoles_to_Scan)
                {
                    migrationBuilder.InsertData(
                                     table: "AspNetUserRoles",
                                     columns: new[] { "UserId", "RoleId" },
                                     values: new object[] { itemUser.Id, itemRole.Id }
                                     );
                }

            }


        }



    }



}
