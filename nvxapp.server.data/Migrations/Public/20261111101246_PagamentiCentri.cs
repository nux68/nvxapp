using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class PagamentiCentri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataPagamento",
                schema: "public",
                table: "Lic_Pagamento",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DealerDescrizione",
                schema: "public",
                table: "Lic_Pagamento",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdDealer",
                schema: "public",
                table: "Lic_Pagamento",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdRendiconto",
                schema: "public",
                table: "Lic_Pagamento",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdRendicontoRettifica",
                schema: "public",
                table: "Lic_Pagamento",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Pag_DealerConfig",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDealer = table.Column<int>(type: "integer", nullable: false),
                    PercentualePiattaforma = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Iban = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    Intestatario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EmailAmministrazione = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pag_DealerConfig", x => x.Id);
                    table.CheckConstraint("CK_Pag_DealerConfig_Percentuale", "\"PercentualePiattaforma\" IS NULL OR \"PercentualePiattaforma\" BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_Pag_DealerConfig_Dealer_IdDealer",
                        column: x => x.IdDealer,
                        principalSchema: "public",
                        principalTable: "Dealer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pag_InvioEmail",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDealer = table.Column<int>(type: "integer", nullable: false),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    Mese = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    IdRendiconto = table.Column<int>(type: "integer", nullable: true),
                    Automatico = table.Column<bool>(type: "boolean", nullable: false),
                    Destinatari = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Esito = table.Column<bool>(type: "boolean", nullable: false),
                    Errore = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pag_InvioEmail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pag_Rendiconto",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDealer = table.Column<int>(type: "integer", nullable: false),
                    DealerDescrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    Mese = table.Column<int>(type: "integer", nullable: false),
                    NumeroPagamenti = table.Column<int>(type: "integer", nullable: false),
                    TotaleIncassato = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    TotaleRettifiche = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    PercentualePiattaforma = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    QuotaPiattaforma = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    QuotaCentro = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Valuta = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    DataConferma = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataLiquidazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RiferimentoBonifico = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroFatturaCentro = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DataFatturaCentro = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pag_Rendiconto", x => x.Id);
                    table.CheckConstraint("CK_Pag_Rendiconto_Mese", "\"Mese\" BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_Pag_Rendiconto_Percentuale", "\"PercentualePiattaforma\" BETWEEN 0 AND 100");
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "AppSetting",
                columns: new[] { "Key", "ModifiedDate", "Value" },
                values: new object[,]
                {
                    { "Pagamenti_GiornoInvioMensile", null, "5" },
                    { "Pagamenti_MeseInvioAnnuale", null, "1" },
                    { "Pagamenti_PercentualePiattaforma", null, "0" },
                    { "Pagamenti_RiepilogoAutomatico", null, "false" }
                });

            // pagamenti gia' presenti (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md §2): centro del cliente oggi (azienda -> studio -> centro
            // oppure studio -> centro) e data di incasso = ultima modifica per quelli andati a buon fine (Completato, Debug).
            // Per i soggetti gia' cancellati il centro resta vuoto: la quota rimane alla piattaforma.
            migrationBuilder.Sql(@"
UPDATE public.""Lic_Pagamento"" p
   SET ""IdDealer"" = fa.""IdDealer"", ""DealerDescrizione"" = d.""Descrizione""
  FROM public.""Company"" c
  JOIN public.""FinancialAdvisor"" fa ON fa.""Id"" = c.""IdFinancialAdvisor""
  JOIN public.""Dealer"" d ON d.""Id"" = fa.""IdDealer""
 WHERE p.""IdCompany"" = c.""Id"" AND p.""IdDealer"" IS NULL;

UPDATE public.""Lic_Pagamento"" p
   SET ""IdDealer"" = fa.""IdDealer"", ""DealerDescrizione"" = d.""Descrizione""
  FROM public.""FinancialAdvisor"" fa
  JOIN public.""Dealer"" d ON d.""Id"" = fa.""IdDealer""
 WHERE p.""IdCompany"" IS NULL AND p.""IdFinancialAdvisor"" = fa.""Id"" AND p.""IdDealer"" IS NULL;

UPDATE public.""Lic_Pagamento""
   SET ""DataPagamento"" = COALESCE(""ModifiedDate"", ""CreationDate"")
 WHERE ""Stato"" IN (2, 5) AND ""DataPagamento"" IS NULL;
");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Pagamento_IdDealer_DataPagamento",
                schema: "public",
                table: "Lic_Pagamento",
                columns: new[] { "IdDealer", "DataPagamento" });

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Pagamento_IdRendiconto",
                schema: "public",
                table: "Lic_Pagamento",
                column: "IdRendiconto");

            migrationBuilder.CreateIndex(
                name: "IX_Lic_Pagamento_IdRendicontoRettifica",
                schema: "public",
                table: "Lic_Pagamento",
                column: "IdRendicontoRettifica");

            migrationBuilder.CreateIndex(
                name: "IX_Pag_DealerConfig_IdDealer",
                schema: "public",
                table: "Pag_DealerConfig",
                column: "IdDealer",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pag_InvioEmail_IdDealer_Anno_Mese_Tipo",
                schema: "public",
                table: "Pag_InvioEmail",
                columns: new[] { "IdDealer", "Anno", "Mese", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_Pag_Rendiconto_Anno_Mese",
                schema: "public",
                table: "Pag_Rendiconto",
                columns: new[] { "Anno", "Mese" });

            migrationBuilder.CreateIndex(
                name: "IX_Pag_Rendiconto_IdDealer_Anno_Mese",
                schema: "public",
                table: "Pag_Rendiconto",
                columns: new[] { "IdDealer", "Anno", "Mese" },
                unique: true,
                filter: "\"Stato\" <> 3");

            migrationBuilder.AddForeignKey(
                name: "FK_Lic_Pagamento_Pag_Rendiconto_IdRendiconto",
                schema: "public",
                table: "Lic_Pagamento",
                column: "IdRendiconto",
                principalSchema: "public",
                principalTable: "Pag_Rendiconto",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Lic_Pagamento_Pag_Rendiconto_IdRendicontoRettifica",
                schema: "public",
                table: "Lic_Pagamento",
                column: "IdRendicontoRettifica",
                principalSchema: "public",
                principalTable: "Pag_Rendiconto",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lic_Pagamento_Pag_Rendiconto_IdRendiconto",
                schema: "public",
                table: "Lic_Pagamento");

            migrationBuilder.DropForeignKey(
                name: "FK_Lic_Pagamento_Pag_Rendiconto_IdRendicontoRettifica",
                schema: "public",
                table: "Lic_Pagamento");

            migrationBuilder.DropTable(
                name: "Pag_DealerConfig",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Pag_InvioEmail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Pag_Rendiconto",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Lic_Pagamento_IdDealer_DataPagamento",
                schema: "public",
                table: "Lic_Pagamento");

            migrationBuilder.DropIndex(
                name: "IX_Lic_Pagamento_IdRendiconto",
                schema: "public",
                table: "Lic_Pagamento");

            migrationBuilder.DropIndex(
                name: "IX_Lic_Pagamento_IdRendicontoRettifica",
                schema: "public",
                table: "Lic_Pagamento");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Pagamenti_GiornoInvioMensile");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Pagamenti_MeseInvioAnnuale");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Pagamenti_PercentualePiattaforma");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Pagamenti_RiepilogoAutomatico");

            migrationBuilder.DropColumn(
                name: "DataPagamento",
                schema: "public",
                table: "Lic_Pagamento");

            migrationBuilder.DropColumn(
                name: "DealerDescrizione",
                schema: "public",
                table: "Lic_Pagamento");

            migrationBuilder.DropColumn(
                name: "IdDealer",
                schema: "public",
                table: "Lic_Pagamento");

            migrationBuilder.DropColumn(
                name: "IdRendiconto",
                schema: "public",
                table: "Lic_Pagamento");

            migrationBuilder.DropColumn(
                name: "IdRendicontoRettifica",
                schema: "public",
                table: "Lic_Pagamento");
        }
    }
}
