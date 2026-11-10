using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class InitPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "AppSetting",
                schema: "public",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSetting", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "public",
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
                schema: "public",
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
                name: "CompanyRegistrationRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Mail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PowerAdminUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AdminUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdCompany = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRegistrationRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyStorageHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    Mese = table.Column<int>(type: "integer", nullable: false),
                    Pacchetto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    QuotaMB = table.Column<int>(type: "integer", nullable: false),
                    UsedBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileCount = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyStorageHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dealer",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PuoGestireListini = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealer", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "StoragePlan",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QuotaMB = table.Column<int>(type: "integer", nullable: false),
                    MaxFileMB = table.Column<int>(type: "integer", nullable: true),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Predefinito = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoragePlan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationType = table.Column<int>(type: "integer", nullable: true),
                    Codice = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialAdvisor",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "Dealer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lic_Listino",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDealer = table.Column<int>(type: "integer", nullable: true),
                    TipoSoggetto = table.Column<int>(type: "integer", nullable: false),
                    Ambito = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Valuta = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DisponibileSuCompany = table.Column<bool>(type: "boolean", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lic_Listino", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lic_Listino_Dealer_IdDealer",
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
                    MainUser = table.Column<bool>(type: "boolean", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "Ticket",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTicketType = table.Column<int>(type: "integer", nullable: false),
                    Oggetto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsersOpener = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    OpenerRoleId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    OpenerLevel = table.Column<int>(type: "integer", nullable: false),
                    IdDealer = table.Column<int>(type: "integer", nullable: true),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: true),
                    IdCompany = table.Column<int>(type: "integer", nullable: true),
                    CurrentRoleId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CurrentLevel = table.Column<int>(type: "integer", nullable: false),
                    CurrentIdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AiAnswerCount = table.Column<int>(type: "integer", nullable: false),
                    AiLevel = table.Column<int>(type: "integer", nullable: true),
                    AiPendingSince = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ticket_TicketType_IdTicketType",
                        column: x => x.IdTicketType,
                        principalSchema: "public",
                        principalTable: "TicketType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketTypeLevel",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTicketType = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTypeLevel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTypeLevel_TicketType_IdTicketType",
                        column: x => x.IdTicketType,
                        principalSchema: "public",
                        principalTable: "TicketType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                        principalSchema: "public",
                        principalTable: "FinancialAdvisor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialAdvisorApplication",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: false),
                    ApplicationType = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ActivationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeactivationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAdvisorApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialAdvisorApplication_FinancialAdvisor_IdFinancialAdv~",
                        column: x => x.IdFinancialAdvisor,
                        principalSchema: "public",
                        principalTable: "FinancialAdvisor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFinancialAdvisor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "text", nullable: false),
                    MainUser = table.Column<bool>(type: "boolean", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false),
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
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFinancialAdvisor_FinancialAdvisor_IdFinancialAdvisor",
                        column: x => x.IdFinancialAdvisor,
                        principalSchema: "public",
                        principalTable: "FinancialAdvisor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lic_Piano",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdListino = table.Column<int>(type: "integer", nullable: false),
                    Livello = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    PrezzoMensile = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PrezzoAnnuale = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxUtenti = table.Column<int>(type: "integer", nullable: true),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lic_Piano", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lic_Piano_Lic_Listino_IdListino",
                        column: x => x.IdListino,
                        principalSchema: "public",
                        principalTable: "Lic_Listino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketCronology",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTicket = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IdAspNetUsersAuthor = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReturnRoleId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ReturnLevel = table.Column<int>(type: "integer", nullable: true),
                    ReturnUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IdTicketType = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCronology", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketCronology_Ticket_IdTicket",
                        column: x => x.IdTicket,
                        principalSchema: "public",
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyApplication",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    ApplicationType = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ActivationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeactivationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyApplication_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyMenuRestriction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMenuRestriction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyMenuRestriction_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyStorage",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    IdStoragePlan = table.Column<int>(type: "integer", nullable: false),
                    MaxFileMB = table.Column<int>(type: "integer", nullable: true),
                    WarningPercent = table.Column<int>(type: "integer", nullable: true),
                    UsedBytes = table.Column<long>(type: "bigint", nullable: false),
                    ReservedBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileCount = table.Column<int>(type: "integer", nullable: false),
                    LastRecalc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastWarningPercent = table.Column<int>(type: "integer", nullable: false),
                    LastWarningDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyStorage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyStorage_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyStorage_StoragePlan_IdStoragePlan",
                        column: x => x.IdStoragePlan,
                        principalSchema: "public",
                        principalTable: "StoragePlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Section = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    IdDealer = table.Column<int>(type: "integer", nullable: true),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: true),
                    IdCompany = table.Column<int>(type: "integer", nullable: true),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Contenuto = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    File = table.Column<byte[]>(type: "bytea", nullable: true),
                    ApplicationType = table.Column<int>(type: "integer", nullable: true),
                    IncludeSubLevels = table.Column<bool>(type: "boolean", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                    table.CheckConstraint("CK_Document_Node", "(\"Level\" IN (0, 1) AND \"IdDealer\" IS NULL AND \"IdFinancialAdvisor\" IS NULL AND \"IdCompany\" IS NULL) OR (\"Level\" = 2 AND \"IdFinancialAdvisor\" IS NULL AND \"IdCompany\" IS NULL) OR (\"Level\" = 3 AND \"IdDealer\" IS NULL AND \"IdCompany\" IS NULL) OR (\"Level\" = 4 AND \"IdDealer\" IS NULL AND \"IdFinancialAdvisor\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_Document_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Document_Dealer_IdDealer",
                        column: x => x.IdDealer,
                        principalSchema: "public",
                        principalTable: "Dealer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Document_FinancialAdvisor_IdFinancialAdvisor",
                        column: x => x.IdFinancialAdvisor,
                        principalSchema: "public",
                        principalTable: "FinancialAdvisor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketTypeAiSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTicketType = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    IdDealer = table.Column<int>(type: "integer", nullable: true),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: true),
                    IdCompany = table.Column<int>(type: "integer", nullable: true),
                    AiEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AiMaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    UseParentDocuments = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTypeAiSetting", x => x.Id);
                    table.CheckConstraint("CK_TicketTypeAiSetting_Node", "(\"Level\" IN (0, 1) AND \"IdDealer\" IS NULL AND \"IdFinancialAdvisor\" IS NULL AND \"IdCompany\" IS NULL) OR (\"Level\" = 2 AND \"IdFinancialAdvisor\" IS NULL AND \"IdCompany\" IS NULL) OR (\"Level\" = 3 AND \"IdDealer\" IS NULL AND \"IdCompany\" IS NULL) OR (\"Level\" = 4 AND \"IdDealer\" IS NULL AND \"IdFinancialAdvisor\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_TicketTypeAiSetting_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketTypeAiSetting_Dealer_IdDealer",
                        column: x => x.IdDealer,
                        principalSchema: "public",
                        principalTable: "Dealer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketTypeAiSetting_FinancialAdvisor_IdFinancialAdvisor",
                        column: x => x.IdFinancialAdvisor,
                        principalSchema: "public",
                        principalTable: "FinancialAdvisor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketTypeAiSetting_TicketType_IdTicketType",
                        column: x => x.IdTicketType,
                        principalSchema: "public",
                        principalTable: "TicketType",
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
                    MainUser = table.Column<bool>(type: "boolean", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Lic_Abbonamento",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: true),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: true),
                    IdListino = table.Column<int>(type: "integer", nullable: true),
                    IdPiano = table.Column<int>(type: "integer", nullable: true),
                    Ambito = table.Column<int>(type: "integer", nullable: false),
                    Livello = table.Column<int>(type: "integer", nullable: false),
                    DescrizionePiano = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Periodicita = table.Column<int>(type: "integer", nullable: false),
                    PrezzoBloccato = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Valuta = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MaxUtentiBloccato = table.Column<int>(type: "integer", nullable: true),
                    DisponibileSuCompany = table.Column<bool>(type: "boolean", nullable: false),
                    IdDealerListino = table.Column<int>(type: "integer", nullable: true),
                    DataInizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataScadenza = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    RinnovoAutomatico = table.Column<bool>(type: "boolean", nullable: false),
                    Metodo = table.Column<int>(type: "integer", nullable: false),
                    RiferimentoGateway = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DataUltimoAvviso = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lic_Abbonamento", x => x.Id);
                    table.CheckConstraint("CK_Lic_Abbonamento_Soggetto", "(\"IdCompany\" IS NULL) <> (\"IdFinancialAdvisor\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_Lic_Abbonamento_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lic_Abbonamento_FinancialAdvisor_IdFinancialAdvisor",
                        column: x => x.IdFinancialAdvisor,
                        principalSchema: "public",
                        principalTable: "FinancialAdvisor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lic_Abbonamento_Lic_Listino_IdListino",
                        column: x => x.IdListino,
                        principalSchema: "public",
                        principalTable: "Lic_Listino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Lic_Abbonamento_Lic_Piano_IdPiano",
                        column: x => x.IdPiano,
                        principalSchema: "public",
                        principalTable: "Lic_Piano",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TicketAttach",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTicketCronology = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    File = table.Column<byte[]>(type: "bytea", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAttach", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketAttach_TicketCronology_IdTicketCronology",
                        column: x => x.IdTicketCronology,
                        principalSchema: "public",
                        principalTable: "TicketCronology",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTicketType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDocument = table.Column<int>(type: "integer", nullable: false),
                    IdTicketType = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTicketType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentTicketType_Document_IdDocument",
                        column: x => x.IdDocument,
                        principalSchema: "public",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentTicketType_TicketType_IdTicketType",
                        column: x => x.IdTicketType,
                        principalSchema: "public",
                        principalTable: "TicketType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketCronologyDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTicketCronology = table.Column<int>(type: "integer", nullable: false),
                    IdDocument = table.Column<int>(type: "integer", nullable: true),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCronologyDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketCronologyDocument_Document_IdDocument",
                        column: x => x.IdDocument,
                        principalSchema: "public",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TicketCronologyDocument_TicketCronology_IdTicketCronology",
                        column: x => x.IdTicketCronology,
                        principalSchema: "public",
                        principalTable: "TicketCronology",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lic_Pagamento",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAbbonamento = table.Column<int>(type: "integer", nullable: true),
                    IdCompany = table.Column<int>(type: "integer", nullable: true),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: true),
                    Soggetto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Ambito = table.Column<int>(type: "integer", nullable: false),
                    Livello = table.Column<int>(type: "integer", nullable: false),
                    Importo = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Valuta = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Periodicita = table.Column<int>(type: "integer", nullable: false),
                    DataInizioCoperta = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataFineCoperta = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Metodo = table.Column<int>(type: "integer", nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    RiferimentoGateway = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Debug = table.Column<bool>(type: "boolean", nullable: false),
                    IdDealerListino = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lic_Pagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lic_Pagamento_Lic_Abbonamento_IdAbbonamento",
                        column: x => x.IdAbbonamento,
                        principalSchema: "public",
                        principalTable: "Lic_Abbonamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "AppSetting",
                columns: new[] { "Key", "ModifiedDate", "Value" },
                values: new object[,]
                {
                    { "Licenze_Attive", null, "true" },
                    { "Licenze_AvvisoGiorni", null, "10" },
                    { "Licenze_GraceGiorni", null, "7" },
                    { "Licenze_MaxUtenti_Free", null, "10" },
                    { "Licenze_MaxUtenti_Pro", null, "" },
                    { "Licenze_MaxUtenti_Standard", null, "50" },
                    { "Storage_MaxFileMB", null, "50" },
                    { "Storage_WarningPercent", null, "80" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "public",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "public",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "public",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "public",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "public",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "public",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "public",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_Descrizione",
                schema: "public",
                table: "Company",
                column: "Descrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_IdFinancialAdvisor",
                schema: "public",
                table: "Company",
                column: "IdFinancialAdvisor");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyApplication_IdCompany_ApplicationType",
                schema: "public",
                table: "CompanyApplication",
                columns: new[] { "IdCompany", "ApplicationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMenuRestriction_IdCompany_Url_Role",
                schema: "public",
                table: "CompanyMenuRestriction",
                columns: new[] { "IdCompany", "Url", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRegistrationRequest_Descrizione",
                schema: "public",
                table: "CompanyRegistrationRequest",
                column: "Descrizione");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRegistrationRequest_TokenHash",
                schema: "public",
                table: "CompanyRegistrationRequest",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyStorage_IdCompany",
                schema: "public",
                table: "CompanyStorage",
                column: "IdCompany",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyStorage_IdStoragePlan",
                schema: "public",
                table: "CompanyStorage",
                column: "IdStoragePlan");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyStorageHistory_IdCompany_Anno_Mese",
                schema: "public",
                table: "CompanyStorageHistory",
                columns: new[] { "IdCompany", "Anno", "Mese" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dealer_Descrizione",
                schema: "public",
                table: "Dealer",
                column: "Descrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeletionLog_Data",
                schema: "public",
                table: "DeletionLog",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Document_IdCompany",
                schema: "public",
                table: "Document",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_Document_IdDealer",
                schema: "public",
                table: "Document",
                column: "IdDealer");

            migrationBuilder.CreateIndex(
                name: "IX_Document_IdFinancialAdvisor",
                schema: "public",
                table: "Document",
                column: "IdFinancialAdvisor");

            migrationBuilder.CreateIndex(
                name: "IX_Document_Section_Level_IdDealer_IdFinancialAdvisor_IdCompan~",
                schema: "public",
                table: "Document",
                columns: new[] { "Section", "Level", "IdDealer", "IdFinancialAdvisor", "IdCompany", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTicketType_IdDocument_IdTicketType",
                schema: "public",
                table: "DocumentTicketType",
                columns: new[] { "IdDocument", "IdTicketType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTicketType_IdTicketType",
                schema: "public",
                table: "DocumentTicketType",
                column: "IdTicketType");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAdvisor_Descrizione",
                schema: "public",
                table: "FinancialAdvisor",
                column: "Descrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAdvisor_IdDealer",
                schema: "public",
                table: "FinancialAdvisor",
                column: "IdDealer");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAdvisorApplication_IdFinancialAdvisor_ApplicationT~",
                schema: "public",
                table: "FinancialAdvisorApplication",
                columns: new[] { "IdFinancialAdvisor", "ApplicationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Abbonamento_DataScadenza",
                schema: "public",
                table: "Lic_Abbonamento",
                column: "DataScadenza");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Abbonamento_IdCompany_Ambito",
                schema: "public",
                table: "Lic_Abbonamento",
                columns: new[] { "IdCompany", "Ambito" });

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Abbonamento_IdFinancialAdvisor_Ambito",
                schema: "public",
                table: "Lic_Abbonamento",
                columns: new[] { "IdFinancialAdvisor", "Ambito" });

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Abbonamento_IdListino",
                schema: "public",
                table: "Lic_Abbonamento",
                column: "IdListino");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Abbonamento_IdPiano",
                schema: "public",
                table: "Lic_Abbonamento",
                column: "IdPiano");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Abbonamento_RiferimentoGateway",
                schema: "public",
                table: "Lic_Abbonamento",
                column: "RiferimentoGateway",
                unique: true,
                filter: "\"RiferimentoGateway\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Listino_Dealer_Attivo",
                schema: "public",
                table: "Lic_Listino",
                columns: new[] { "IdDealer", "TipoSoggetto", "Ambito" },
                unique: true,
                filter: "\"Attivo\" AND \"IdDealer\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Listino_Globale_Attivo",
                schema: "public",
                table: "Lic_Listino",
                columns: new[] { "TipoSoggetto", "Ambito" },
                unique: true,
                filter: "\"Attivo\" AND \"IdDealer\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Pagamento_IdAbbonamento",
                schema: "public",
                table: "Lic_Pagamento",
                column: "IdAbbonamento");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Pagamento_IdCompany",
                schema: "public",
                table: "Lic_Pagamento",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Pagamento_IdFinancialAdvisor",
                schema: "public",
                table: "Lic_Pagamento",
                column: "IdFinancialAdvisor");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Pagamento_RiferimentoGateway",
                schema: "public",
                table: "Lic_Pagamento",
                column: "RiferimentoGateway",
                unique: true,
                filter: "\"RiferimentoGateway\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Piano_IdListino_Livello",
                schema: "public",
                table: "Lic_Piano",
                columns: new[] { "IdListino", "Livello" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoragePlan_Descrizione",
                schema: "public",
                table: "StoragePlan",
                column: "Descrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoragePlan_Predefinito",
                schema: "public",
                table: "StoragePlan",
                column: "Predefinito",
                unique: true,
                filter: "\"Predefinito\"");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_CurrentIdAspNetUsers",
                schema: "public",
                table: "Ticket",
                column: "CurrentIdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_CurrentLevel_State",
                schema: "public",
                table: "Ticket",
                columns: new[] { "CurrentLevel", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_IdAspNetUsersOpener",
                schema: "public",
                table: "Ticket",
                column: "IdAspNetUsersOpener");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_IdCompany",
                schema: "public",
                table: "Ticket",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_IdDealer",
                schema: "public",
                table: "Ticket",
                column: "IdDealer");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_IdFinancialAdvisor",
                schema: "public",
                table: "Ticket",
                column: "IdFinancialAdvisor");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_IdTicketType",
                schema: "public",
                table: "Ticket",
                column: "IdTicketType");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttach_IdTicketCronology",
                schema: "public",
                table: "TicketAttach",
                column: "IdTicketCronology");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCronology_IdTicket",
                schema: "public",
                table: "TicketCronology",
                column: "IdTicket");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCronology_UserId",
                schema: "public",
                table: "TicketCronology",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCronologyDocument_IdDocument",
                schema: "public",
                table: "TicketCronologyDocument",
                column: "IdDocument");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCronologyDocument_IdTicketCronology",
                schema: "public",
                table: "TicketCronologyDocument",
                column: "IdTicketCronology");

            migrationBuilder.CreateIndex(
                name: "IX_TicketType_Codice",
                schema: "public",
                table: "TicketType",
                column: "Codice",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeAiSetting_IdCompany",
                schema: "public",
                table: "TicketTypeAiSetting",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeAiSetting_IdDealer",
                schema: "public",
                table: "TicketTypeAiSetting",
                column: "IdDealer");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeAiSetting_IdFinancialAdvisor",
                schema: "public",
                table: "TicketTypeAiSetting",
                column: "IdFinancialAdvisor");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeAiSetting_IdTicketType_IdCompany",
                schema: "public",
                table: "TicketTypeAiSetting",
                columns: new[] { "IdTicketType", "IdCompany" },
                unique: true,
                filter: "\"IdCompany\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeAiSetting_IdTicketType_IdDealer",
                schema: "public",
                table: "TicketTypeAiSetting",
                columns: new[] { "IdTicketType", "IdDealer" },
                unique: true,
                filter: "\"IdDealer\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeAiSetting_IdTicketType_IdFinancialAdvisor",
                schema: "public",
                table: "TicketTypeAiSetting",
                columns: new[] { "IdTicketType", "IdFinancialAdvisor" },
                unique: true,
                filter: "\"IdFinancialAdvisor\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeAiSetting_IdTicketType_Level",
                schema: "public",
                table: "TicketTypeAiSetting",
                columns: new[] { "IdTicketType", "Level" },
                unique: true,
                filter: "\"IdDealer\" IS NULL AND \"IdFinancialAdvisor\" IS NULL AND \"IdCompany\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeLevel_IdTicketType_Level",
                schema: "public",
                table: "TicketTypeLevel",
                columns: new[] { "IdTicketType", "Level" },
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

            migrationBuilder.CreateIndex(
                name: "IX_UserFinancialAdvisor_IdAspNetUsers",
                schema: "public",
                table: "UserFinancialAdvisor",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_UserFinancialAdvisor_IdFinancialAdvisor_IdAspNetUsers",
                schema: "public",
                table: "UserFinancialAdvisor",
                columns: new[] { "IdFinancialAdvisor", "IdAspNetUsers" },
                unique: true);

            // dati iniziali: ruoli, utenti base e gerarchia di prova (Dealer/FinancialAdvisor/Company/utenti, piu' DEMO)
            PopolateDB.PopolateDB_InitDB_UP(migrationBuilder);

            // tipi di richiesta iniziali dei ticket di assistenza
            PopolateTicketType.PopolateTicketType_UP(migrationBuilder);

            // pacchetti di spazio iniziali (1, 5 e 20 GB, predefinito 1 GB)
            PopolateStorage.PopolateStorage_UP(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CompanyApplication",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CompanyMenuRestriction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CompanyRegistrationRequest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CompanyStorage",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CompanyStorageHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DeletionLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DocumentTicketType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinancialAdvisorApplication",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Lic_Pagamento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketAttach",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketCronologyDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketTypeAiSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketTypeLevel",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UserCompany",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UserDealer",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UserFinancialAdvisor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StoragePlan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Lic_Abbonamento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Document",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketCronology",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Lic_Piano",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Company",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Ticket",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Lic_Listino",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinancialAdvisor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dealer",
                schema: "public");
        }
    }
}
