using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Assets
{
    /// <inheritdoc />
    public partial class AstBeni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ast_Categoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Categoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Categoria_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ast_Bene",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    CodiceInventario = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IdAst_Categoria = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MarcaModello = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroSerie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataAcquisto = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Valore = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    ScadenzaGaranzia = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: true),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Bene", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Bene_Ast_Categoria_IdAst_Categoria",
                        column: x => x.IdAst_Categoria,
                        principalTable: "Ast_Categoria",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ast_Bene_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_Bene_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ast_BeneAssegnazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAst_Bene = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    DataConsegna = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CondizioniConsegna = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataRestituzione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CondizioniRestituzione = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataAvvisoCessazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_BeneAssegnazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_BeneAssegnazione_Ast_Bene_IdAst_Bene",
                        column: x => x.IdAst_Bene,
                        principalTable: "Ast_Bene",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_BeneAssegnazione_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Bene_IdAst_Categoria",
                table: "Ast_Bene",
                column: "IdAst_Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Bene_IdAz_Anagrafica_CodiceInventario",
                table: "Ast_Bene",
                columns: new[] { "IdAz_Anagrafica", "CodiceInventario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Bene_IdAz_Sedi",
                table: "Ast_Bene",
                column: "IdAz_Sedi");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_BeneAssegnazione_Aperta",
                table: "Ast_BeneAssegnazione",
                column: "IdAst_Bene",
                unique: true,
                filter: "\"DataRestituzione\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_BeneAssegnazione_IdAst_Bene_DataConsegna",
                table: "Ast_BeneAssegnazione",
                columns: new[] { "IdAst_Bene", "DataConsegna" });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_BeneAssegnazione_IdDip_Anagrafica",
                table: "Ast_BeneAssegnazione",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Categoria_IdAz_Anagrafica_Descrizione",
                table: "Ast_Categoria",
                columns: new[] { "IdAz_Anagrafica", "Descrizione" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ast_BeneAssegnazione");

            migrationBuilder.DropTable(
                name: "Ast_Bene");

            migrationBuilder.DropTable(
                name: "Ast_Categoria");
        }
    }
}
