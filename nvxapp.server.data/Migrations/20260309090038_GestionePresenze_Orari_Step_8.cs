using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCausale_Lavoro_Strao",
                schema: "public",
                table: "Par_ProfiloOrario",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdCausale_Lavoro_Suppl",
                schema: "public",
                table: "Par_ProfiloOrario",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Alle_Arrotondamento",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Alle_Arrotondamento_Verso",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Dalle_Arrotondamento",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Dalle_Arrotondamento_Verso",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrario_IdCausale_Lavoro_Strao",
                schema: "public",
                table: "Par_ProfiloOrario",
                column: "IdCausale_Lavoro_Strao");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrario_IdCausale_Lavoro_Suppl",
                schema: "public",
                table: "Par_ProfiloOrario",
                column: "IdCausale_Lavoro_Suppl");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_ProfiloOrario_Par_Causali_IdCausale_Lavoro_Strao",
                schema: "public",
                table: "Par_ProfiloOrario",
                column: "IdCausale_Lavoro_Strao",
                principalSchema: "public",
                principalTable: "Par_Causali",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Par_ProfiloOrario_Par_Causali_IdCausale_Lavoro_Suppl",
                schema: "public",
                table: "Par_ProfiloOrario",
                column: "IdCausale_Lavoro_Suppl",
                principalSchema: "public",
                principalTable: "Par_Causali",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Par_ProfiloOrario_Par_Causali_IdCausale_Lavoro_Strao",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropForeignKey(
                name: "FK_Par_ProfiloOrario_Par_Causali_IdCausale_Lavoro_Suppl",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropIndex(
                name: "IX_Par_ProfiloOrario_IdCausale_Lavoro_Strao",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropIndex(
                name: "IX_Par_ProfiloOrario_IdCausale_Lavoro_Suppl",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropColumn(
                name: "IdCausale_Lavoro_Strao",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropColumn(
                name: "IdCausale_Lavoro_Suppl",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropColumn(
                name: "Alle_Arrotondamento",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "Alle_Arrotondamento_Verso",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "Dalle_Arrotondamento",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "Dalle_Arrotondamento_Verso",
                schema: "public",
                table: "Par_OrarioIntervalloHH");
        }
    }
}
