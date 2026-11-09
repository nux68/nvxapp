using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class Licenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PuoGestireListini",
                schema: "public",
                table: "Dealer",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                    { "Licenze_Attive", null, "false" },
                    { "Licenze_AvvisoGiorni", null, "10" },
                    { "Licenze_GraceGiorni", null, "7" },
                    { "Licenze_MaxUtenti_Free", null, "10" },
                    { "Licenze_MaxUtenti_Pro", null, "" },
                    { "Licenze_MaxUtenti_Standard", null, "50" }
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lic_Pagamento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Lic_Abbonamento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Lic_Piano",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Lic_Listino",
                schema: "public");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_Attive");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_AvvisoGiorni");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_GraceGiorni");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_MaxUtenti_Free");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_MaxUtenti_Pro");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_MaxUtenti_Standard");

            migrationBuilder.DropColumn(
                name: "PuoGestireListini",
                schema: "public",
                table: "Dealer");
        }
    }
}
