using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCausale_HH_Lav_MonteOre",
                schema: "public",
                table: "Par_Orario",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimbratureTipo",
                schema: "public",
                table: "Par_Orario",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Par_Orario_IdCausale_HH_Lav_MonteOre",
                schema: "public",
                table: "Par_Orario",
                column: "IdCausale_HH_Lav_MonteOre");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_Orario_Par_Causali_IdCausale_HH_Lav_MonteOre",
                schema: "public",
                table: "Par_Orario",
                column: "IdCausale_HH_Lav_MonteOre",
                principalSchema: "public",
                principalTable: "Par_Causali",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Par_Orario_Par_Causali_IdCausale_HH_Lav_MonteOre",
                schema: "public",
                table: "Par_Orario");

            migrationBuilder.DropIndex(
                name: "IX_Par_Orario_IdCausale_HH_Lav_MonteOre",
                schema: "public",
                table: "Par_Orario");

            migrationBuilder.DropColumn(
                name: "IdCausale_HH_Lav_MonteOre",
                schema: "public",
                table: "Par_Orario");

            migrationBuilder.DropColumn(
                name: "TimbratureTipo",
                schema: "public",
                table: "Par_Orario");
        }
    }
}
