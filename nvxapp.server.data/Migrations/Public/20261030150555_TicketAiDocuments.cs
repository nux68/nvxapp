using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class TicketAiDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AiAnswerCount",
                schema: "public",
                table: "Ticket",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AiLevel",
                schema: "public",
                table: "Ticket",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiPendingSince",
                schema: "public",
                table: "Ticket",
                type: "timestamp without time zone",
                nullable: true);

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

            // TicketType.AiResolution diventa l'impostazione di default (un tentativo) dei livelli abilitati
            // che ricevono il ticket con l'AI: Dev, Dealer, FinancialAdvisor, Company
            migrationBuilder.Sql(
                "INSERT INTO public.\"TicketTypeAiSetting\" (\"IdTicketType\", \"Level\", \"AiEnabled\", \"AiMaxAttempts\", \"CreationDate\", \"ModifiedDate\") " +
                "SELECT l.\"IdTicketType\", l.\"Level\", true, 1, now(), now() " +
                "FROM public.\"TicketTypeLevel\" l JOIN public.\"TicketType\" t ON t.\"Id\" = l.\"IdTicketType\" " +
                "WHERE t.\"AiResolution\" AND l.\"Level\" IN (1, 2, 3, 4);");

            migrationBuilder.DropColumn(
                name: "AiResolution",
                schema: "public",
                table: "TicketType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentTicketType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketCronologyDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketTypeAiSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Document",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "AiAnswerCount",
                schema: "public",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "AiLevel",
                schema: "public",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "AiPendingSince",
                schema: "public",
                table: "Ticket");

            migrationBuilder.AddColumn<bool>(
                name: "AiResolution",
                schema: "public",
                table: "TicketType",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
