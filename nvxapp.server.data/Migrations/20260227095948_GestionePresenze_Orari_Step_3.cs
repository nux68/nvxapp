using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdPar_Orario",
                schema: "public",
                table: "Par_ProfiloOrarioGG",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ZOrder",
                schema: "public",
                table: "Par_ProfiloOrarioGG",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrarioGG_IdPar_Orario",
                schema: "public",
                table: "Par_ProfiloOrarioGG",
                column: "IdPar_Orario");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_ProfiloOrarioGG_Par_Orario_IdPar_Orario",
                schema: "public",
                table: "Par_ProfiloOrarioGG",
                column: "IdPar_Orario",
                principalSchema: "public",
                principalTable: "Par_Orario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Par_ProfiloOrarioGG_Par_Orario_IdPar_Orario",
                schema: "public",
                table: "Par_ProfiloOrarioGG");

            migrationBuilder.DropIndex(
                name: "IX_Par_ProfiloOrarioGG_IdPar_Orario",
                schema: "public",
                table: "Par_ProfiloOrarioGG");

            migrationBuilder.DropColumn(
                name: "IdPar_Orario",
                schema: "public",
                table: "Par_ProfiloOrarioGG");

            migrationBuilder.DropColumn(
                name: "ZOrder",
                schema: "public",
                table: "Par_ProfiloOrarioGG");
        }
    }
}
