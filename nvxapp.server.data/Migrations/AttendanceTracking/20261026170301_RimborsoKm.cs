using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class RimborsoKm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Par_TipoSpesa",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdDip_RapportoAutovetture",
                table: "Dip_GG_NotaSpese",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Km",
                table: "Dip_GG_NotaSpese",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Dip_RapportoAutovetture",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Targa = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    RimborsoKM = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_RapportoAutovetture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_RapportoAutovetture_Dip_RapportoLavoro_IdDip_RapportoLa~",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_NotaSpese_IdDip_RapportoAutovetture",
                table: "Dip_GG_NotaSpese",
                column: "IdDip_RapportoAutovetture");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_RapportoAutovetture_IdDip_RapportoLavoro",
                table: "Dip_RapportoAutovetture",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.AddForeignKey(
                name: "FK_Dip_GG_NotaSpese_Dip_RapportoAutovetture_IdDip_RapportoAuto~",
                table: "Dip_GG_NotaSpese",
                column: "IdDip_RapportoAutovetture",
                principalTable: "Dip_RapportoAutovetture",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dip_GG_NotaSpese_Dip_RapportoAutovetture_IdDip_RapportoAuto~",
                table: "Dip_GG_NotaSpese");

            migrationBuilder.DropTable(
                name: "Dip_RapportoAutovetture");

            migrationBuilder.DropIndex(
                name: "IX_Dip_GG_NotaSpese_IdDip_RapportoAutovetture",
                table: "Dip_GG_NotaSpese");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Par_TipoSpesa");

            migrationBuilder.DropColumn(
                name: "IdDip_RapportoAutovetture",
                table: "Dip_GG_NotaSpese");

            migrationBuilder.DropColumn(
                name: "Km",
                table: "Dip_GG_NotaSpese");
        }
    }
}
