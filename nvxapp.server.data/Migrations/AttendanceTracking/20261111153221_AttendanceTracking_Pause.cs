using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class AttendanceTracking_Pause : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AnnullaPausa",
                table: "Par_Giustificativi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Min_PausaDetratta",
                table: "Dip_Periodo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Min_PausaDetratta",
                table: "Dip_GG_Result",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Min_PausaRetribuita",
                table: "Dip_GG_Result",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Min_PausaRilevata",
                table: "Dip_GG_Result",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Par_OrarioPausa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPar_Orario = table.Column<int>(type: "integer", nullable: false),
                    NumPausa = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Modalita = table.Column<int>(type: "integer", nullable: false),
                    DurataMin = table.Column<int>(type: "integer", nullable: false),
                    DurataMaxMin = table.Column<int>(type: "integer", nullable: true),
                    SogliaMin = table.Column<int>(type: "integer", nullable: false),
                    FinestraDalle = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    FinestraAlle = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    RetribuitaMin = table.Column<int>(type: "integer", nullable: false),
                    IdCausale_Pausa = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_OrarioPausa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_OrarioPausa_Par_Causali_IdCausale_Pausa",
                        column: x => x.IdCausale_Pausa,
                        principalTable: "Par_Causali",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Par_OrarioPausa_Par_Orario_IdPar_Orario",
                        column: x => x.IdPar_Orario,
                        principalTable: "Par_Orario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Par_OrarioPausa_IdCausale_Pausa",
                table: "Par_OrarioPausa",
                column: "IdCausale_Pausa");

            migrationBuilder.CreateIndex(
                name: "IX_Par_OrarioPausa_IdPar_Orario",
                table: "Par_OrarioPausa",
                column: "IdPar_Orario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Par_OrarioPausa");

            migrationBuilder.DropColumn(
                name: "AnnullaPausa",
                table: "Par_Giustificativi");

            migrationBuilder.DropColumn(
                name: "Min_PausaDetratta",
                table: "Dip_Periodo");

            migrationBuilder.DropColumn(
                name: "Min_PausaDetratta",
                table: "Dip_GG_Result");

            migrationBuilder.DropColumn(
                name: "Min_PausaRetribuita",
                table: "Dip_GG_Result");

            migrationBuilder.DropColumn(
                name: "Min_PausaRilevata",
                table: "Dip_GG_Result");
        }
    }
}
