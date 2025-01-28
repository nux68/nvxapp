using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Migrations
{
    public class PopolateDB
    {

        protected readonly IAspNetUsersRepository _aspNetUsersRepository;


        public PopolateDB(IAspNetUsersRepository aspNetUsersRepository)
        {
            _aspNetUsersRepository = aspNetUsersRepository;
        }

        public static void PopolateDB_InitDB_UP(MigrationBuilder migrationBuilder)
        {
            var roles = new[]
                {
                    "SuperUser",
                    "PowerAdmin",
                    "Admin",
                    "DomainPowerAdmin",
                    "DomainAdmin",
                    "User"
                };

            foreach (var role in roles)
            {
                migrationBuilder.InsertData(
                    table: "AspNetRoles",
                    columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                    values: new object[] { Guid.NewGuid().ToString(), role, role.ToUpper(), Guid.NewGuid().ToString() }
                );
            }



            // Creazione utente
            var userId = Guid.NewGuid().ToString();
            var hasher = new PasswordHasher<IdentityUser>();
            var passwordHash = hasher.HashPassword(null, "1234");


            

            migrationBuilder.InsertData(
                                            table: "AspNetUsers",
                                            columns: new[]
                                            {
                                                "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                                                "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "LockoutEnabled",
                                                "TwoFactorEnabled","PhoneNumberConfirmed","AccessFailedCount"
                                            },
                                            values: new object[]
                                            {
                                                userId, "nvx", "NVX", "nello68@hotmail.com", "NELLO68@HOTMAIL.COM",
                                                true, passwordHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), false,
                                                false,false, 0
                                            }
            );




            //migrationBuilder.Sql($@"

            //                    INSERT INTO AspNetUserRoles (""UserId"", ""RoleId"")
            //                    SELECT '{userId}', ""Id"" FROM ""AspNetRoles"" WHERE ""Name"" = 'SuperUser'
            //                    UNION ALL
            //                    SELECT '{userId}', ""Id"" FROM ""AspNetRoles"" WHERE ""Name"" = 'PowerAdmin'
            //                    UNION ALL
            //                    SELECT '{userId}', ""Id"" FROM ""AspNetRoles"" WHERE ""Name"" = 'Admin'
            //                    UNION ALL
            //                    SELECT '{userId}', ""Id"" FROM ""AspNetRoles"" WHERE ""Name"" = 'DomainPowerAdmin'
            //                    UNION ALL
            //                    SELECT '{userId}', ""Id"" FROM ""AspNetRoles"" WHERE ""Name"" = 'DomainAdmin'
            //                    UNION ALL
            //                    SELECT '{userId}', ""Id"" FROM ""AspNetRoles"" WHERE ""Name"" = 'User';
            //                ");

            //// Collegamento dei ruoli all'utente nella tabella AspNetUserRoles
            //foreach (var role in roles)
            //{
            //    var roleIdQuery = $"SELECT Id FROM AspNetRoles WHERE Name = '{role}'";

            //    migrationBuilder.Sql($@"
            //                            INSERT INTO AspNetUserRoles (UserId, RoleId)
            //                            SELECT '{userId}', Id
            //                            FROM AspNetRoles
            //                            WHERE Name = '{role}'
            //                        ");
            //}
            ////UserId = table.Column<string>(type: "text", nullable: false),
            ////        RoleId = table.Column<string>(type: "text", nullable: false)

            /** USER 2 **/

            userId = Guid.NewGuid().ToString();
            migrationBuilder.InsertData(
                                            table: "AspNetUsers",
                                            columns: new[]
                                            {
                                                "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                                                "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "LockoutEnabled",
                                                "TwoFactorEnabled","PhoneNumberConfirmed","AccessFailedCount"
                                            },
                                            values: new object[]
                                            {
                                                userId, "nvx2", "NVX2", "nello68@hotmail.com", "NELLO68@HOTMAIL.COM",
                                                true, passwordHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), false,
                                                false,false, 0
                                            }
                                        );


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
