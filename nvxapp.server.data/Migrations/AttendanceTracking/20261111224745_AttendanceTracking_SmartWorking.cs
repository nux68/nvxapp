using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class AttendanceTracking_SmartWorking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LavoroAgile",
                table: "Par_Giustificativi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Dip_AccordoSW",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Dal = table.Column<DateTime>(type: "date", nullable: false),
                    Al = table.Column<DateTime>(type: "date", nullable: false),
                    MaxGiorniSettimana = table.Column<int>(type: "integer", nullable: false),
                    GiorniConsentiti = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataRichiestaFirma = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataFirma = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_AccordoSW", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_AccordoSW_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dip_AccordoSW_IdDip_RapportoLavoro_Dal",
                table: "Dip_AccordoSW",
                columns: new[] { "IdDip_RapportoLavoro", "Dal" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dip_AccordoSW");

            migrationBuilder.DropColumn(
                name: "LavoroAgile",
                table: "Par_Giustificativi");
        }
    }
}
