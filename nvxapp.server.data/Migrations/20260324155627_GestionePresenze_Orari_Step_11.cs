using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCausale",
                schema: "public",
                table: "Par_Giustificativi",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Segno",
                schema: "public",
                table: "Par_Giustificativi",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Par_Giustificativi_IdCausale",
                schema: "public",
                table: "Par_Giustificativi",
                column: "IdCausale");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_Giustificativi_Par_Causali_IdCausale",
                schema: "public",
                table: "Par_Giustificativi",
                column: "IdCausale",
                principalSchema: "public",
                principalTable: "Par_Causali",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Par_Giustificativi_Par_Causali_IdCausale",
                schema: "public",
                table: "Par_Giustificativi");

            migrationBuilder.DropIndex(
                name: "IX_Par_Giustificativi_IdCausale",
                schema: "public",
                table: "Par_Giustificativi");

            migrationBuilder.DropColumn(
                name: "IdCausale",
                schema: "public",
                table: "Par_Giustificativi");

            migrationBuilder.DropColumn(
                name: "Segno",
                schema: "public",
                table: "Par_Giustificativi");
        }
    }
}
