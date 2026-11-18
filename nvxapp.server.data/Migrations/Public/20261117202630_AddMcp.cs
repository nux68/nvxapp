using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class AddMcp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "McpAccessToken",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAspNetUsersLogin = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IdDealer = table.Column<int>(type: "integer", nullable: true),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: true),
                    IdCompany = table.Column<int>(type: "integer", nullable: true),
                    Tenant = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TokenPrefix = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpAccessToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpAccessToken_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_McpAccessToken_AspNetUsers_IdAspNetUsersLogin",
                        column: x => x.IdAspNetUsersLogin,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_McpAccessToken_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "McpCompanySetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    MaxLevel = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpCompanySetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpCompanySetting_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "McpCallLog",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IdMcpAccessToken = table.Column<int>(type: "integer", nullable: true),
                    TokenName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdAspNetUsersLogin = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UserNameLogin = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IdCompany = table.Column<int>(type: "integer", nullable: true),
                    Tool = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Arguments = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpCallLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpCallLog_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_McpCallLog_McpAccessToken_IdMcpAccessToken",
                        column: x => x.IdMcpAccessToken,
                        principalSchema: "public",
                        principalTable: "McpAccessToken",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_McpAccessToken_IdAspNetUsers",
                schema: "public",
                table: "McpAccessToken",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_McpAccessToken_IdAspNetUsersLogin",
                schema: "public",
                table: "McpAccessToken",
                column: "IdAspNetUsersLogin");

            migrationBuilder.CreateIndex(
                name: "IX_McpAccessToken_IdCompany",
                schema: "public",
                table: "McpAccessToken",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_McpAccessToken_TokenHash",
                schema: "public",
                table: "McpAccessToken",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_McpCallLog_IdAspNetUsersLogin_Data",
                schema: "public",
                table: "McpCallLog",
                columns: new[] { "IdAspNetUsersLogin", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_McpCallLog_IdCompany_Data",
                schema: "public",
                table: "McpCallLog",
                columns: new[] { "IdCompany", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_McpCallLog_IdMcpAccessToken",
                schema: "public",
                table: "McpCallLog",
                column: "IdMcpAccessToken");

            migrationBuilder.CreateIndex(
                name: "IX_McpCompanySetting_IdCompany",
                schema: "public",
                table: "McpCompanySetting",
                column: "IdCompany",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "McpCallLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McpCompanySetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McpAccessToken",
                schema: "public");
        }
    }
}
