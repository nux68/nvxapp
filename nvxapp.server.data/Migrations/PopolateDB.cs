using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
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
            public string Id { get; set; }
            public string Name { get; set; }
        }

        class Data_AspNetUsers
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
        }

        public static void PopolateDB_InitDB_UP(MigrationBuilder migrationBuilder)
        {

            //user
            List<Data_AspNetUsers> data_AspNetUsers = new List<Data_AspNetUsers>();
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "NVX", Email = "nello68@hotmail.com" });
            data_AspNetUsers.Add(new Data_AspNetUsers() { Id = Guid.NewGuid().ToString(), UserName = "NVX2", Email = "nello68@hotmail.com" });

            //AspNetRoles
            List<string> roleUser = AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.User);

            List<Data_AspNetRoles> data_AspNetRoles_All = new List<Data_AspNetRoles>();
            List<string> roleAll = AspNetUsersDataUtil.Get_AspNetRoles(AspNetUsersDataUtil.AspNetRolesGroup.All);
            foreach (var item in roleAll)
            {
                data_AspNetRoles_All.Add(new Data_AspNetRoles() { Id = Guid.NewGuid().ToString(), Name = item });
            }


            ///*TO DB*/

            //AspNetRoles
            List<Data_AspNetRoles> data_AspNetRoles_to_Scan = data_AspNetRoles_All.Where( x=> roleAll.Contains(x.Name)).ToList();

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
            var passwordHash = hasher.HashPassword(null, "1234");

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


                if (itemUser.UserName == "NVX")
                    data_AspNetRoles_to_Scan = data_AspNetRoles_All;
                else
                    data_AspNetRoles_to_Scan = data_AspNetRoles_All.Where(x => roleUser.Contains(x.Name)).ToList(); 

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

        //public static void PopolateDB_InitDB_DOWN(MigrationBuilder migrationBuilder)
        //{
        //    // Rimuovi i dati in ordine inverso
        //    migrationBuilder.Sql("DELETE FROM AspNetUserRoles");
        //    migrationBuilder.Sql("DELETE FROM AspNetUsers WHERE UserName = 'nvx'");
        //    migrationBuilder.Sql("DELETE FROM AspNetRoles");
        //}

    }



}
