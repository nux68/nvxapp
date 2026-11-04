using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class ApplicationsAndStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.InsertData(
                schema: "public",
                table: "AppSetting",
                columns: new[] { "Key", "ModifiedDate", "Value" },
                values: new object[,]
                {
                    { "Storage_MaxFileMB", null, "50" },
                    { "Storage_WarningPercent", null, "80" }
                });

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
                name: "IX_FinancialAdvisorApplication_IdFinancialAdvisor_ApplicationT~",
                schema: "public",
                table: "FinancialAdvisorApplication",
                columns: new[] { "IdFinancialAdvisor", "ApplicationType" },
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

            // pacchetti di spazio iniziali (1, 5 e 20 GB, predefinito 1 GB)
            PopolateStorage.PopolateStorage_UP(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyStorage",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CompanyStorageHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinancialAdvisorApplication",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StoragePlan",
                schema: "public");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Storage_MaxFileMB");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Storage_WarningPercent");
        }
    }
}
