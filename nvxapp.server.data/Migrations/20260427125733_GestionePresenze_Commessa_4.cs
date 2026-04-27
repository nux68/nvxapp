using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Commessa_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdAz_SubCommessaAttivita_MonteOre",
                schema: "public",
                table: "Par_Orario",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Par_OrarioIntervalloHH_IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                column: "IdAz_SubCommessaAttivita");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Orario_IdAz_SubCommessaAttivita_MonteOre",
                schema: "public",
                table: "Par_Orario",
                column: "IdAz_SubCommessaAttivita_MonteOre");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_Orario_Az_SubCommessaAttivita_IdAz_SubCommessaAttivita_~",
                schema: "public",
                table: "Par_Orario",
                column: "IdAz_SubCommessaAttivita_MonteOre",
                principalSchema: "public",
                principalTable: "Az_SubCommessaAttivita",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Par_OrarioIntervalloHH_Az_SubCommessaAttivita_IdAz_SubComme~",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
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
                name: "FK_Par_Orario_Az_SubCommessaAttivita_IdAz_SubCommessaAttivita_~",
                schema: "public",
                table: "Par_Orario");

            migrationBuilder.DropForeignKey(
                name: "FK_Par_OrarioIntervalloHH_Az_SubCommessaAttivita_IdAz_SubComme~",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropIndex(
                name: "IX_Par_OrarioIntervalloHH_IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropIndex(
                name: "IX_Par_Orario_IdAz_SubCommessaAttivita_MonteOre",
                schema: "public",
                table: "Par_Orario");

            migrationBuilder.DropColumn(
                name: "IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "IdAz_SubCommessaAttivita_MonteOre",
                schema: "public",
                table: "Par_Orario");
        }
    }
}
