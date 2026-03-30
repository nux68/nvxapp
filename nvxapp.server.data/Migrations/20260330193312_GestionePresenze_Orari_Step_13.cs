using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCausale_HH_Lav",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Par_OrarioIntervalloHH_IdCausale_HH_Lav",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                column: "IdCausale_HH_Lav");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_OrarioIntervalloHH_Par_Causali_IdCausale_HH_Lav",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                column: "IdCausale_HH_Lav",
                principalSchema: "public",
                principalTable: "Par_Causali",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Par_OrarioIntervalloHH_Par_Causali_IdCausale_HH_Lav",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropIndex(
                name: "IX_Par_OrarioIntervalloHH_IdCausale_HH_Lav",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "IdCausale_HH_Lav",
                schema: "public",
                table: "Par_OrarioIntervalloHH");
        }
    }
}
