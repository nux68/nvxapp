using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class DealerAndCompany_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dealer",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdDealer = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Company_Dealer_IdDealer",
                        column: x => x.IdDealer,
                        principalSchema: "public",
                        principalTable: "Dealer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDealer",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDealer = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDealer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDealer_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDealer_Dealer_IdDealer",
                        column: x => x.IdDealer,
                        principalSchema: "public",
                        principalTable: "Dealer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCompany",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompany", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCompany_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCompany_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Company_Descrizione",
                schema: "public",
                table: "Company",
                column: "Descrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_IdDealer",
                schema: "public",
                table: "Company",
                column: "IdDealer");

            migrationBuilder.CreateIndex(
                name: "IX_Dealer_Descrizione",
                schema: "public",
                table: "Dealer",
                column: "Descrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCompany_IdAspNetUsers",
                schema: "public",
                table: "UserCompany",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompany_IdCompany_IdAspNetUsers",
                schema: "public",
                table: "UserCompany",
                columns: new[] { "IdCompany", "IdAspNetUsers" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDealer_IdAspNetUsers",
                schema: "public",
                table: "UserDealer",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_UserDealer_IdDealer_IdAspNetUsers",
                schema: "public",
                table: "UserDealer",
                columns: new[] { "IdDealer", "IdAspNetUsers" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCompany",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UserDealer",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Company",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dealer",
                schema: "public");
        }
    }
}
