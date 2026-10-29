using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class ExportCau_Docs_Verso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoFile",
                table: "Par_ExportCau");

            migrationBuilder.AddColumn<int>(
                name: "Verso",
                table: "Par_ExportCau_Causali",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NoteAI",
                table: "Par_ExportCau",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RicettaAI",
                table: "Par_ExportCau",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RicettaHash",
                table: "Par_ExportCau",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Par_ExportCau_Docs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPar_ExportCau = table.Column<int>(type: "integer", nullable: false),
                    NomeFile = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Dimensione = table.Column<int>(type: "integer", nullable: false),
                    Contenuto = table.Column<byte[]>(type: "bytea", nullable: false),
                    Testo = table.Column<string>(type: "text", nullable: true),
                    Descrizione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ExportCau_Docs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ExportCau_Docs_Par_ExportCau_IdPar_ExportCau",
                        column: x => x.IdPar_ExportCau,
                        principalTable: "Par_ExportCau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Par_ExportCau_Docs_IdPar_ExportCau",
                table: "Par_ExportCau_Docs",
                column: "IdPar_ExportCau");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Par_ExportCau_Docs");

            migrationBuilder.DropColumn(
                name: "Verso",
                table: "Par_ExportCau_Causali");

            migrationBuilder.DropColumn(
                name: "NoteAI",
                table: "Par_ExportCau");

            migrationBuilder.DropColumn(
                name: "RicettaAI",
                table: "Par_ExportCau");

            migrationBuilder.DropColumn(
                name: "RicettaHash",
                table: "Par_ExportCau");

            migrationBuilder.AddColumn<int>(
                name: "TipoFile",
                table: "Par_ExportCau",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
