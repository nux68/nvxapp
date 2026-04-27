using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Commessa_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Par_ProfiloOrarioGG_Az_SubCommessaAttivita_IdAz_SubCommessa~",
                schema: "public",
                table: "Par_ProfiloOrarioGG");

            migrationBuilder.DropIndex(
                name: "IX_Par_ProfiloOrarioGG_IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Par_ProfiloOrarioGG");

            migrationBuilder.DropColumn(
                name: "IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Par_ProfiloOrarioGG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Par_ProfiloOrarioGG",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrarioGG_IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Par_ProfiloOrarioGG",
                column: "IdAz_SubCommessaAttivita");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_ProfiloOrarioGG_Az_SubCommessaAttivita_IdAz_SubCommessa~",
                schema: "public",
                table: "Par_ProfiloOrarioGG",
                column: "IdAz_SubCommessaAttivita",
                principalSchema: "public",
                principalTable: "Az_SubCommessaAttivita",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
