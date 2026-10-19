using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class NotaSpese : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descrizione",
                table: "Dip_GG_NotaSpese",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdPar_TipoSpesa",
                table: "Dip_GG_NotaSpese",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Importo",
                table: "Dip_GG_NotaSpese",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Dip_GG_NotaSpesa_Allegato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_GG_NotaSpesa = table.Column<int>(type: "integer", nullable: false),
                    NomeFile = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Dimensione = table.Column<int>(type: "integer", nullable: false),
                    Contenuto = table.Column<byte[]>(type: "bytea", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_NotaSpesa_Allegato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_NotaSpesa_Allegato_Dip_GG_NotaSpese_IdDip_GG_NotaSpe~",
                        column: x => x.IdDip_GG_NotaSpesa,
                        principalTable: "Dip_GG_NotaSpese",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_TipoSpesa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_TipoSpesa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_TipoSpesa_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_NotaSpese_IdPar_TipoSpesa",
                table: "Dip_GG_NotaSpese",
                column: "IdPar_TipoSpesa");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_NotaSpesa_Allegato_IdDip_GG_NotaSpesa",
                table: "Dip_GG_NotaSpesa_Allegato",
                column: "IdDip_GG_NotaSpesa");

            migrationBuilder.CreateIndex(
                name: "IX_Par_TipoSpesa_IdAz_Anagrafica",
                table: "Par_TipoSpesa",
                column: "IdAz_Anagrafica");

            migrationBuilder.AddForeignKey(
                name: "FK_Dip_GG_NotaSpese_Par_TipoSpesa_IdPar_TipoSpesa",
                table: "Dip_GG_NotaSpese",
                column: "IdPar_TipoSpesa",
                principalTable: "Par_TipoSpesa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dip_GG_NotaSpese_Par_TipoSpesa_IdPar_TipoSpesa",
                table: "Dip_GG_NotaSpese");

            migrationBuilder.DropTable(
                name: "Dip_GG_NotaSpesa_Allegato");

            migrationBuilder.DropTable(
                name: "Par_TipoSpesa");

            migrationBuilder.DropIndex(
                name: "IX_Dip_GG_NotaSpese_IdPar_TipoSpesa",
                table: "Dip_GG_NotaSpese");

            migrationBuilder.DropColumn(
                name: "Descrizione",
                table: "Dip_GG_NotaSpese");

            migrationBuilder.DropColumn(
                name: "IdPar_TipoSpesa",
                table: "Dip_GG_NotaSpese");

            migrationBuilder.DropColumn(
                name: "Importo",
                table: "Dip_GG_NotaSpese");
        }
    }
}
