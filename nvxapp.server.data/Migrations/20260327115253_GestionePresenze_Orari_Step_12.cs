using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdGiustificativo_Assenza_Ingiust",
                schema: "public",
                table: "Par_ProfiloOrario",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrario_IdGiustificativo_Assenza_Ingiust",
                schema: "public",
                table: "Par_ProfiloOrario",
                column: "IdGiustificativo_Assenza_Ingiust");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_ProfiloOrario_Par_Giustificativi_IdGiustificativo_Assen~",
                schema: "public",
                table: "Par_ProfiloOrario",
                column: "IdGiustificativo_Assenza_Ingiust",
                principalSchema: "public",
                principalTable: "Par_Giustificativi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Par_ProfiloOrario_Par_Giustificativi_IdGiustificativo_Assen~",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropIndex(
                name: "IX_Par_ProfiloOrario_IdGiustificativo_Assenza_Ingiust",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropColumn(
                name: "IdGiustificativo_Assenza_Ingiust",
                schema: "public",
                table: "Par_ProfiloOrario");
        }
    }
}
