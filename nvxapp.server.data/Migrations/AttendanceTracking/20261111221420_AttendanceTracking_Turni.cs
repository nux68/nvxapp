using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class AttendanceTracking_Turni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Par_Turno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdPar_Orario = table.Column<int>(type: "integer", nullable: true),
                    Colore = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_Turno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_Turno_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Par_Turno_Par_Orario_IdPar_Orario",
                        column: x => x.IdPar_Orario,
                        principalTable: "Par_Orario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Az_SediRepartoTurno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_SediReparto = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Turno = table.Column<int>(type: "integer", nullable: false),
                    MinPersone = table.Column<int>(type: "integer", nullable: false),
                    GiorniSettimana = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_SediRepartoTurno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_SediRepartoTurno_Az_SediReparto_IdAz_SediReparto",
                        column: x => x.IdAz_SediReparto,
                        principalTable: "Az_SediReparto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Az_SediRepartoTurno_Par_Turno_IdPar_Turno",
                        column: x => x.IdPar_Turno,
                        principalTable: "Par_Turno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_Turno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "date", nullable: false),
                    IdPar_Turno = table.Column<int>(type: "integer", nullable: false),
                    IdAz_SediReparto = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_Turno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_Turno_Az_SediReparto_IdAz_SediReparto",
                        column: x => x.IdAz_SediReparto,
                        principalTable: "Az_SediReparto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Dip_Turno_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_Turno_Par_Turno_IdPar_Turno",
                        column: x => x.IdPar_Turno,
                        principalTable: "Par_Turno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Dip_TurnoScambio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_Turno = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Turno_Collega = table.Column<int>(type: "integer", nullable: true),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    IdDip_RapportoLavoro_Collega = table.Column<int>(type: "integer", nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    IdDip_GG_Richiesta = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DataRisposta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_TurnoScambio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_TurnoScambio_Dip_GG_Richieste_IdDip_GG_Richiesta",
                        column: x => x.IdDip_GG_Richiesta,
                        principalTable: "Dip_GG_Richieste",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Dip_TurnoScambio_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_TurnoScambio_Dip_RapportoLavoro_IdDip_RapportoLavoro_Co~",
                        column: x => x.IdDip_RapportoLavoro_Collega,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_TurnoScambio_Dip_Turno_IdDip_Turno",
                        column: x => x.IdDip_Turno,
                        principalTable: "Dip_Turno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_TurnoScambio_Dip_Turno_IdDip_Turno_Collega",
                        column: x => x.IdDip_Turno_Collega,
                        principalTable: "Dip_Turno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediRepartoTurno_IdAz_SediReparto_IdPar_Turno",
                table: "Az_SediRepartoTurno",
                columns: new[] { "IdAz_SediReparto", "IdPar_Turno" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediRepartoTurno_IdPar_Turno",
                table: "Az_SediRepartoTurno",
                column: "IdPar_Turno");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Turno_IdAz_SediReparto_Data",
                table: "Dip_Turno",
                columns: new[] { "IdAz_SediReparto", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Turno_IdDip_RapportoLavoro_Data",
                table: "Dip_Turno",
                columns: new[] { "IdDip_RapportoLavoro", "Data" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Turno_IdPar_Turno",
                table: "Dip_Turno",
                column: "IdPar_Turno");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_TurnoScambio_IdDip_GG_Richiesta",
                table: "Dip_TurnoScambio",
                column: "IdDip_GG_Richiesta");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_TurnoScambio_IdDip_RapportoLavoro",
                table: "Dip_TurnoScambio",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_TurnoScambio_IdDip_RapportoLavoro_Collega",
                table: "Dip_TurnoScambio",
                column: "IdDip_RapportoLavoro_Collega");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_TurnoScambio_IdDip_Turno",
                table: "Dip_TurnoScambio",
                column: "IdDip_Turno");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_TurnoScambio_IdDip_Turno_Collega",
                table: "Dip_TurnoScambio",
                column: "IdDip_Turno_Collega");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Turno_IdAz_Anagrafica",
                table: "Par_Turno",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Turno_IdPar_Orario",
                table: "Par_Turno",
                column: "IdPar_Orario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Az_SediRepartoTurno");

            migrationBuilder.DropTable(
                name: "Dip_TurnoScambio");

            migrationBuilder.DropTable(
                name: "Dip_Turno");

            migrationBuilder.DropTable(
                name: "Par_Turno");
        }
    }
}
