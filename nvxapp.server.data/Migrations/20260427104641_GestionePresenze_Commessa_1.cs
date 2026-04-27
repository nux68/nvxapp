using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Commessa_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_GG_Timbrature",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Timbrature_IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_GG_Timbrature",
                column: "IdAz_SubCommessaAttivita");

            migrationBuilder.AddForeignKey(
                name: "FK_Dip_GG_Timbrature_Az_SubCommessaAttivita_IdAz_SubCommessaAt~",
                schema: "public",
                table: "Dip_GG_Timbrature",
                column: "IdAz_SubCommessaAttivita",
                principalSchema: "public",
                principalTable: "Az_SubCommessaAttivita",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dip_GG_Timbrature_Az_SubCommessaAttivita_IdAz_SubCommessaAt~",
                schema: "public",
                table: "Dip_GG_Timbrature");

            migrationBuilder.DropIndex(
                name: "IX_Dip_GG_Timbrature_IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_GG_Timbrature");

            migrationBuilder.DropColumn(
                name: "IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_GG_Timbrature");
        }
    }
}
