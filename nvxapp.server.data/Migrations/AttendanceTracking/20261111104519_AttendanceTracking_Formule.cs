using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class AttendanceTracking_Formule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Par_Formule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Titolo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Descrizione = table.Column<string>(type: "text", nullable: true),
                    Testo = table.Column<string>(type: "text", nullable: true),
                    Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_Formule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_Formule_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_ProfiloFormule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ProfiloFormule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloFormule_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_FormuleLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Giorno = table.Column<DateTime>(type: "date", nullable: false),
                    IdPar_Formule = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Messaggio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_FormuleLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_FormuleLog_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_FormuleLog_Par_Formule_IdPar_Formule",
                        column: x => x.IdPar_Formule,
                        principalTable: "Par_Formule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Dip_ProfiloFormule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    IdPar_ProfiloFormule = table.Column<int>(type: "integer", nullable: false),
                    Dal = table.Column<DateTime>(type: "date", nullable: false),
                    Al = table.Column<DateTime>(type: "date", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_ProfiloFormule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_ProfiloFormule_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_ProfiloFormule_Par_ProfiloFormule_IdPar_ProfiloFormule",
                        column: x => x.IdPar_ProfiloFormule,
                        principalTable: "Par_ProfiloFormule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Par_ProfiloFormuleDett",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPar_ProfiloFormule = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Formule = table.Column<int>(type: "integer", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Dal = table.Column<DateTime>(type: "date", nullable: false),
                    Al = table.Column<DateTime>(type: "date", nullable: false),
                    Sospesa = table.Column<bool>(type: "boolean", nullable: false),
                    Fase = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ProfiloFormuleDett", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloFormuleDett_Par_Formule_IdPar_Formule",
                        column: x => x.IdPar_Formule,
                        principalTable: "Par_Formule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloFormuleDett_Par_ProfiloFormule_IdPar_ProfiloForm~",
                        column: x => x.IdPar_ProfiloFormule,
                        principalTable: "Par_ProfiloFormule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_FormuleLog_IdDip_RapportoLavoro_Giorno",
                table: "Dip_GG_FormuleLog",
                columns: new[] { "IdDip_RapportoLavoro", "Giorno" });

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_FormuleLog_IdPar_Formule",
                table: "Dip_GG_FormuleLog",
                column: "IdPar_Formule");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_ProfiloFormule_IdDip_RapportoLavoro",
                table: "Dip_ProfiloFormule",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_ProfiloFormule_IdPar_ProfiloFormule",
                table: "Dip_ProfiloFormule",
                column: "IdPar_ProfiloFormule");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Formule_IdAz_Anagrafica_Codice",
                table: "Par_Formule",
                columns: new[] { "IdAz_Anagrafica", "Codice" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloFormule_IdAz_Anagrafica_Codice",
                table: "Par_ProfiloFormule",
                columns: new[] { "IdAz_Anagrafica", "Codice" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloFormuleDett_IdPar_Formule",
                table: "Par_ProfiloFormuleDett",
                column: "IdPar_Formule");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloFormuleDett_IdPar_ProfiloFormule",
                table: "Par_ProfiloFormuleDett",
                column: "IdPar_ProfiloFormule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dip_GG_FormuleLog");

            migrationBuilder.DropTable(
                name: "Dip_ProfiloFormule");

            migrationBuilder.DropTable(
                name: "Par_ProfiloFormuleDett");

            migrationBuilder.DropTable(
                name: "Par_Formule");

            migrationBuilder.DropTable(
                name: "Par_ProfiloFormule");
        }
    }
}
