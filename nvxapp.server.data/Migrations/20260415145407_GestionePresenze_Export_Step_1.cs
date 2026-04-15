using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Export_Step_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Stato",
                schema: "public",
                table: "Dip_GG_Result",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "Par_ExportCau",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TipoFile = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ExportCau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ExportCau_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalSchema: "public",
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_ExportCau_Causali",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPar_ExportCau = table.Column<int>(type: "integer", nullable: false),
                    IdCausale = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TipoElaborazione = table.Column<int>(type: "integer", nullable: false),
                    TipoUnita = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ExportCau_Causali", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ExportCau_Causali_Par_Causali_IdCausale",
                        column: x => x.IdCausale,
                        principalSchema: "public",
                        principalTable: "Par_Causali",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Par_ExportCau_Causali_Par_ExportCau_IdPar_ExportCau",
                        column: x => x.IdPar_ExportCau,
                        principalSchema: "public",
                        principalTable: "Par_ExportCau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Par_ExportCau_IdAz_Anagrafica",
                schema: "public",
                table: "Par_ExportCau",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ExportCau_Causali_IdCausale",
                schema: "public",
                table: "Par_ExportCau_Causali",
                column: "IdCausale");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ExportCau_Causali_IdPar_ExportCau",
                schema: "public",
                table: "Par_ExportCau_Causali",
                column: "IdPar_ExportCau");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Par_ExportCau_Causali",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Par_ExportCau",
                schema: "public");

            migrationBuilder.AlterColumn<int>(
                name: "Stato",
                schema: "public",
                table: "Dip_GG_Result",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
