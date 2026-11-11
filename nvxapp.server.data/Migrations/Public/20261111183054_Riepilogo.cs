using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class Riepilogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Riepilogo",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    IdCompany = table.Column<int>(type: "integer", nullable: true),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: true),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Dal = table.Column<DateTime>(type: "date", nullable: false),
                    Al = table.Column<DateTime>(type: "date", nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Testo = table.Column<string>(type: "text", nullable: false),
                    Fatti = table.Column<string>(type: "text", nullable: false),
                    GeneratoConAI = table.Column<bool>(type: "boolean", nullable: false),
                    ProviderAI = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Manuale = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Riepilogo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Riepilogo_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Riepilogo_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Riepilogo_FinancialAdvisor_IdFinancialAdvisor",
                        column: x => x.IdFinancialAdvisor,
                        principalSchema: "public",
                        principalTable: "FinancialAdvisor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Riepilogo_IdAspNetUsers_Al",
                schema: "public",
                table: "Riepilogo",
                columns: new[] { "IdAspNetUsers", "Al" });

            migrationBuilder.CreateIndex(
                name: "IX_Riepilogo_IdCompany_Al",
                schema: "public",
                table: "Riepilogo",
                columns: new[] { "IdCompany", "Al" });

            migrationBuilder.CreateIndex(
                name: "IX_Riepilogo_IdFinancialAdvisor_Al",
                schema: "public",
                table: "Riepilogo",
                columns: new[] { "IdFinancialAdvisor", "Al" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Riepilogo",
                schema: "public");
        }
    }
}
