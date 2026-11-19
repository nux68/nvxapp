using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class AddMcpImpersonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomeIdentity",
                schema: "public",
                table: "McpAccessToken",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImpersonationExpiresAt",
                schema: "public",
                table: "McpAccessToken",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeIdentity",
                schema: "public",
                table: "McpAccessToken");

            migrationBuilder.DropColumn(
                name: "ImpersonationExpiresAt",
                schema: "public",
                table: "McpAccessToken");
        }
    }
}
