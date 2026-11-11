using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Assets
{
    /// <inheritdoc />
    public partial class AstPrenotazioni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrenotazioneAnticipoMaxGiorni",
                table: "Ast_Cfg",
                type: "integer",
                nullable: false,
                defaultValue: 60); // configurazioni gia' presenti: stessi limiti predefiniti di Ast_Cfg

            migrationBuilder.AddColumn<int>(
                name: "PrenotazioneDurataMaxOre",
                table: "Ast_Cfg",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.CreateTable(
                name: "Ast_Risorsa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: true),
                    Capienza = table.Column<int>(type: "integer", nullable: true),
                    IdAst_Veicolo = table.Column<int>(type: "integer", nullable: true),
                    Attiva = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Risorsa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Risorsa_Ast_Veicolo_IdAst_Veicolo",
                        column: x => x.IdAst_Veicolo,
                        principalTable: "Ast_Veicolo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ast_Risorsa_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_Risorsa_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ast_Prenotazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAst_Risorsa = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Inizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Fine = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    DataAnnullamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Prenotazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Prenotazione_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_Prenotazione_Ast_Risorsa_IdAst_Risorsa",
                        column: x => x.IdAst_Risorsa,
                        principalTable: "Ast_Risorsa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Prenotazione_IdAspNetUsers_Inizio",
                table: "Ast_Prenotazione",
                columns: new[] { "IdAspNetUsers", "Inizio" });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Prenotazione_IdAst_Risorsa_Inizio_Fine",
                table: "Ast_Prenotazione",
                columns: new[] { "IdAst_Risorsa", "Inizio", "Fine" });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Risorsa_IdAst_Veicolo",
                table: "Ast_Risorsa",
                column: "IdAst_Veicolo");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Risorsa_IdAz_Anagrafica_Tipo_Descrizione",
                table: "Ast_Risorsa",
                columns: new[] { "IdAz_Anagrafica", "Tipo", "Descrizione" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Risorsa_IdAz_Sedi",
                table: "Ast_Risorsa",
                column: "IdAz_Sedi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ast_Prenotazione");

            migrationBuilder.DropTable(
                name: "Ast_Risorsa");

            migrationBuilder.DropColumn(
                name: "PrenotazioneAnticipoMaxGiorni",
                table: "Ast_Cfg");

            migrationBuilder.DropColumn(
                name: "PrenotazioneDurataMaxOre",
                table: "Ast_Cfg");
        }
    }
}
