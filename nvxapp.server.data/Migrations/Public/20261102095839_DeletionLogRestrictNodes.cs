using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class DeletionLogRestrictNodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_FinancialAdvisor_IdFinancialAdvisor",
                schema: "public",
                table: "Company");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAdvisor_Dealer_IdDealer",
                schema: "public",
                table: "FinancialAdvisor");

            migrationBuilder.CreateTable(
                name: "DeletionLog",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NodeType = table.Column<int>(type: "integer", nullable: false),
                    NodeId = table.Column<int>(type: "integer", nullable: true),
                    IdAspNetUsersDeleted = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Descrizione = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Riepilogo = table.Column<string>(type: "text", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletionLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeletionLog_Data",
                schema: "public",
                table: "DeletionLog",
                column: "Data");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_FinancialAdvisor_IdFinancialAdvisor",
                schema: "public",
                table: "Company",
                column: "IdFinancialAdvisor",
                principalSchema: "public",
                principalTable: "FinancialAdvisor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialAdvisor_Dealer_IdDealer",
                schema: "public",
                table: "FinancialAdvisor",
                column: "IdDealer",
                principalSchema: "public",
                principalTable: "Dealer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_FinancialAdvisor_IdFinancialAdvisor",
                schema: "public",
                table: "Company");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAdvisor_Dealer_IdDealer",
                schema: "public",
                table: "FinancialAdvisor");

            migrationBuilder.DropTable(
                name: "DeletionLog",
                schema: "public");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_FinancialAdvisor_IdFinancialAdvisor",
                schema: "public",
                table: "Company",
                column: "IdFinancialAdvisor",
                principalSchema: "public",
                principalTable: "FinancialAdvisor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialAdvisor_Dealer_IdDealer",
                schema: "public",
                table: "FinancialAdvisor",
                column: "IdDealer",
                principalSchema: "public",
                principalTable: "Dealer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
