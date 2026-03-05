using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdPar_Orario_Festivo",
                schema: "public",
                table: "Par_ProfiloOrario",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrario_IdPar_Orario_Festivo",
                schema: "public",
                table: "Par_ProfiloOrario",
                column: "IdPar_Orario_Festivo");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_ProfiloOrario_Par_Orario_IdPar_Orario_Festivo",
                schema: "public",
                table: "Par_ProfiloOrario",
                column: "IdPar_Orario_Festivo",
                principalSchema: "public",
                principalTable: "Par_Orario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Par_ProfiloOrario_Par_Orario_IdPar_Orario_Festivo",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropIndex(
                name: "IX_Par_ProfiloOrario_IdPar_Orario_Festivo",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropColumn(
                name: "IdPar_Orario_Festivo",
                schema: "public",
                table: "Par_ProfiloOrario");
        }
    }
}
