using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using nvxapp.server.data.Infrastructure;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (SharedSchema.CurrentSchema != "public")
                return;

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dealer",
                schema: SharedSchema.CurrentSchema,
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
                name: "AspNetRoleClaims",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialAdvisor",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDealer = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAdvisor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialAdvisor_Dealer_IdDealer",
                        column: x => x.IdDealer,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "Dealer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDealer",
                schema: SharedSchema.CurrentSchema,
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
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDealer_Dealer_IdDealer",
                        column: x => x.IdDealer,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "Dealer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Schema = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Company_FinancialAdvisor_IdFinancialAdvisor",
                        column: x => x.IdFinancialAdvisor,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "FinancialAdvisor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFinancialAdvisor",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFinancialAdvisor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFinancialAdvisor_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFinancialAdvisor_FinancialAdvisor_IdFinancialAdvisor",
                        column: x => x.IdFinancialAdvisor,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "FinancialAdvisor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCompany",
                schema: SharedSchema.CurrentSchema,
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
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCompany_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: SharedSchema.CurrentSchema,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: SharedSchema.CurrentSchema,
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: SharedSchema.CurrentSchema,
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: SharedSchema.CurrentSchema,
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: SharedSchema.CurrentSchema,
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: SharedSchema.CurrentSchema,
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: SharedSchema.CurrentSchema,
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: SharedSchema.CurrentSchema,
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_Descrizione",
                schema: SharedSchema.CurrentSchema,
                table: "Company",
                column: "Descrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_IdFinancialAdvisor",
                schema: SharedSchema.CurrentSchema,
                table: "Company",
                column: "IdFinancialAdvisor");

            migrationBuilder.CreateIndex(
                name: "IX_Dealer_Descrizione",
                schema: SharedSchema.CurrentSchema,
                table: "Dealer",
                column: "Descrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAdvisor_Descrizione",
                schema: SharedSchema.CurrentSchema,
                table: "FinancialAdvisor",
                column: "Descrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAdvisor_IdDealer",
                schema: SharedSchema.CurrentSchema,
                table: "FinancialAdvisor",
                column: "IdDealer");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompany_IdAspNetUsers",
                schema: SharedSchema.CurrentSchema,
                table: "UserCompany",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompany_IdCompany_IdAspNetUsers",
                schema: SharedSchema.CurrentSchema,
                table: "UserCompany",
                columns: new[] { "IdCompany", "IdAspNetUsers" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDealer_IdAspNetUsers",
                schema: SharedSchema.CurrentSchema,
                table: "UserDealer",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_UserDealer_IdDealer_IdAspNetUsers",
                schema: SharedSchema.CurrentSchema,
                table: "UserDealer",
                columns: new[] { "IdDealer", "IdAspNetUsers" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFinancialAdvisor_IdAspNetUsers",
                schema: SharedSchema.CurrentSchema,
                table: "UserFinancialAdvisor",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_UserFinancialAdvisor_IdFinancialAdvisor_IdAspNetUsers",
                schema: SharedSchema.CurrentSchema,
                table: "UserFinancialAdvisor",
                columns: new[] { "IdFinancialAdvisor", "IdAspNetUsers" },
                unique: true);

            PopolateDB.PopolateDB_InitDB_UP(migrationBuilder);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (SharedSchema.CurrentSchema != "public")
                return;

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "UserCompany",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "UserDealer",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "UserFinancialAdvisor",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "Company",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "FinancialAdvisor",
                schema: SharedSchema.CurrentSchema);

            migrationBuilder.DropTable(
                name: "Dealer",
                schema: SharedSchema.CurrentSchema);
        }
    }
}
