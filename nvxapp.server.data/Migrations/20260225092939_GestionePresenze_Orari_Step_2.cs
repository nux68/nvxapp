using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Par_ProfiloOrarioIntervalloHH_Par_Orario_IdPar_Orario",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Par_ProfiloOrarioIntervalloHH",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH");

            migrationBuilder.RenameTable(
                name: "Par_ProfiloOrarioIntervalloHH",
                schema: "public",
                newName: "Par_OrarioIntervalloHH",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_Par_ProfiloOrarioIntervalloHH_IdPar_Orario",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                newName: "IX_Par_OrarioIntervalloHH_IdPar_Orario");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Par_OrarioIntervalloHH",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_OrarioIntervalloHH_Par_Orario_IdPar_Orario",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                column: "IdPar_Orario",
                principalSchema: "public",
                principalTable: "Par_Orario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Par_OrarioIntervalloHH_Par_Orario_IdPar_Orario",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Par_OrarioIntervalloHH",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.RenameTable(
                name: "Par_OrarioIntervalloHH",
                schema: "public",
                newName: "Par_ProfiloOrarioIntervalloHH",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_Par_OrarioIntervalloHH_IdPar_Orario",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH",
                newName: "IX_Par_ProfiloOrarioIntervalloHH_IdPar_Orario");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Par_ProfiloOrarioIntervalloHH",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_ProfiloOrarioIntervalloHH_Par_Orario_IdPar_Orario",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH",
                column: "IdPar_Orario",
                principalSchema: "public",
                principalTable: "Par_Orario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
