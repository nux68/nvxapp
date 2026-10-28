using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class RegistrationAdminUserNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminUserName",
                schema: "public",
                table: "CompanyRegistrationRequest",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PowerAdminUserName",
                schema: "public",
                table: "CompanyRegistrationRequest",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminUserName",
                schema: "public",
                table: "CompanyRegistrationRequest");

            migrationBuilder.DropColumn(
                name: "PowerAdminUserName",
                schema: "public",
                table: "CompanyRegistrationRequest");
        }
    }
}
