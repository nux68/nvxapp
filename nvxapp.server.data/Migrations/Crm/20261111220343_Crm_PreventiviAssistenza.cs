using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Crm
{
    /// <inheritdoc />
    public partial class Crm_PreventiviAssistenza : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoProdotto",
                table: "Crm_Par_Prodotto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEsito",
                table: "Crm_Offerta",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdOrdineAmministrazione",
                table: "Crm_Offerta",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Crm_Richiesta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: false),
                    IdAz_ClienteContatto = table.Column<int>(type: "integer", nullable: true),
                    Oggetto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Canale = table.Column<int>(type: "integer", nullable: false),
                    Priorita = table.Column<int>(type: "integer", nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsersAssegnatario = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    DataApertura = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataPresaInCarico = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataRisoluzione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataChiusura = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Soluzione = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IdCrm_Opportunita = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Richiesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Richiesta_AspNetUsers_IdAspNetUsersAssegnatario",
                        column: x => x.IdAspNetUsersAssegnatario,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Crm_Richiesta_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Richiesta_Az_ClienteContatto_IdAz_ClienteContatto",
                        column: x => x.IdAz_ClienteContatto,
                        principalTable: "Az_ClienteContatto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Crm_Richiesta_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Crm_Richiesta_Crm_Opportunita_IdCrm_Opportunita",
                        column: x => x.IdCrm_Opportunita,
                        principalTable: "Crm_Opportunita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Crm_RichiestaMessaggio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCrm_Richiesta = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IdAspNetUsersAutore = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Testo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Sistema = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_RichiestaMessaggio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_RichiestaMessaggio_Crm_Richiesta_IdCrm_Richiesta",
                        column: x => x.IdCrm_Richiesta,
                        principalTable: "Crm_Richiesta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Richiesta_IdAspNetUsersAssegnatario",
                table: "Crm_Richiesta",
                column: "IdAspNetUsersAssegnatario");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Richiesta_IdAz_Anagrafica_Anno_Numero",
                table: "Crm_Richiesta",
                columns: new[] { "IdAz_Anagrafica", "Anno", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Richiesta_IdAz_Anagrafica_Stato",
                table: "Crm_Richiesta",
                columns: new[] { "IdAz_Anagrafica", "Stato" });

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Richiesta_IdAz_Cliente",
                table: "Crm_Richiesta",
                column: "IdAz_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Richiesta_IdAz_ClienteContatto",
                table: "Crm_Richiesta",
                column: "IdAz_ClienteContatto");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Richiesta_IdCrm_Opportunita",
                table: "Crm_Richiesta",
                column: "IdCrm_Opportunita");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_RichiestaMessaggio_IdCrm_Richiesta",
                table: "Crm_RichiestaMessaggio",
                column: "IdCrm_Richiesta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Crm_RichiestaMessaggio");

            migrationBuilder.DropTable(
                name: "Crm_Richiesta");

            migrationBuilder.DropColumn(
                name: "TipoProdotto",
                table: "Crm_Par_Prodotto");

            migrationBuilder.DropColumn(
                name: "DataEsito",
                table: "Crm_Offerta");

            migrationBuilder.DropColumn(
                name: "IdOrdineAmministrazione",
                table: "Crm_Offerta");
        }
    }
}
