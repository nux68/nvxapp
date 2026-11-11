using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class RegistrazioneStudio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdFinancialAdvisor",
                schema: "public",
                table: "CompanyRegistrationRequest",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                schema: "public",
                table: "CompanyRegistrationRequest",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdFinancialAdvisor",
                schema: "public",
                table: "CompanyRegistrationRequest");

            migrationBuilder.DropColumn(
                name: "Tipo",
                schema: "public",
                table: "CompanyRegistrationRequest");
        }
    }
}
