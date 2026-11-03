using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Common
{
    /// <inheritdoc />
    public partial class InitCommon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Az_Anagrafica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_Anagrafica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_Anagrafica_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_Cliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Default = table.Column<bool>(type: "boolean", nullable: false),
                    PartitaIva = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    CodiceFiscale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Indirizzo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Cap = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Citta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Provincia = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Nazione = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Pec = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Referente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CodiceDestinatario = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_Cliente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_Cliente_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_ClienteContatto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: false),
                    Cognome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ruolo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Cellulare = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Principale = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_ClienteContatto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_ClienteContatto_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Az_Anagrafica_IdCompany",
                table: "Az_Anagrafica",
                column: "IdCompany",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Az_Cliente_IdAz_Anagrafica",
                table: "Az_Cliente",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Az_ClienteContatto_IdAz_Cliente",
                table: "Az_ClienteContatto",
                column: "IdAz_Cliente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Az_ClienteContatto");

            migrationBuilder.DropTable(
                name: "Az_Cliente");

            migrationBuilder.DropTable(
                name: "Az_Anagrafica");
        }
    }
}
