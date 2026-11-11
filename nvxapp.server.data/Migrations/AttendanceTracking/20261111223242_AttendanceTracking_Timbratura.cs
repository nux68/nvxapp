using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class AttendanceTracking_Timbratura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Az_SediTimbratura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: false),
                    Latitudine = table.Column<double>(type: "double precision", nullable: true),
                    Longitudine = table.Column<double>(type: "double precision", nullable: true),
                    RaggioMetri = table.Column<int>(type: "integer", nullable: false),
                    RegolaGeofence = table.Column<int>(type: "integer", nullable: false),
                    QrAttivo = table.Column<bool>(type: "boolean", nullable: false),
                    QrObbligatorio = table.Column<bool>(type: "boolean", nullable: false),
                    QrSegreto = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    QrDurataSecondi = table.Column<int>(type: "integer", nullable: false),
                    KioskAttivo = table.Column<bool>(type: "boolean", nullable: false),
                    KioskToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DifferitaMaxOre = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_SediTimbratura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_SediTimbratura_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_TimbraturaInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_GG_Timbratura = table.Column<int>(type: "integer", nullable: false),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: true),
                    Origine = table.Column<int>(type: "integer", nullable: false),
                    Differita = table.Column<bool>(type: "boolean", nullable: false),
                    DataDispositivo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataRicezione = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Latitudine = table.Column<double>(type: "double precision", nullable: true),
                    Longitudine = table.Column<double>(type: "double precision", nullable: true),
                    PrecisioneMetri = table.Column<double>(type: "double precision", nullable: true),
                    DistanzaMetri = table.Column<double>(type: "double precision", nullable: true),
                    EsitoGeofence = table.Column<int>(type: "integer", nullable: false),
                    QrSlot = table.Column<long>(type: "bigint", nullable: true),
                    IdAspNetUsers_Operatore = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_TimbraturaInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_TimbraturaInfo_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Dip_GG_TimbraturaInfo_Dip_GG_Timbrature_IdDip_GG_Timbratura",
                        column: x => x.IdDip_GG_Timbratura,
                        principalTable: "Dip_GG_Timbrature",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_TimbraturaPin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    PinHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Badge = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_TimbraturaPin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_TimbraturaPin_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediTimbratura_IdAz_Sedi",
                table: "Az_SediTimbratura",
                column: "IdAz_Sedi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_TimbraturaInfo_IdAz_Sedi_QrSlot_IdDip_RapportoLavoro",
                table: "Dip_GG_TimbraturaInfo",
                columns: new[] { "IdAz_Sedi", "QrSlot", "IdDip_RapportoLavoro" },
                unique: true,
                filter: "\"QrSlot\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_TimbraturaInfo_IdDip_GG_Timbratura",
                table: "Dip_GG_TimbraturaInfo",
                column: "IdDip_GG_Timbratura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_TimbraturaPin_IdAz_Anagrafica_Badge",
                table: "Dip_TimbraturaPin",
                columns: new[] { "IdAz_Anagrafica", "Badge" },
                unique: true,
                filter: "\"Badge\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_TimbraturaPin_IdAz_Anagrafica_PinHash",
                table: "Dip_TimbraturaPin",
                columns: new[] { "IdAz_Anagrafica", "PinHash" },
                unique: true,
                filter: "\"PinHash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_TimbraturaPin_IdDip_Anagrafica",
                table: "Dip_TimbraturaPin",
                column: "IdDip_Anagrafica",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Az_SediTimbratura");

            migrationBuilder.DropTable(
                name: "Dip_GG_TimbraturaInfo");

            migrationBuilder.DropTable(
                name: "Dip_TimbraturaPin");
        }
    }
}
