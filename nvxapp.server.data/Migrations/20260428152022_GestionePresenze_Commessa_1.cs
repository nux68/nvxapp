using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Commessa_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Az_SubCommessaAttivitaId",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Az_SubCommessaAttivitaId",
                schema: "public",
                table: "Par_Orario",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_RapportoLavoro",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_GG_Timbrature",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Par_OrarioIntervalloHH_Az_SubCommessaAttivitaId",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                column: "Az_SubCommessaAttivitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Orario_Az_SubCommessaAttivitaId",
                schema: "public",
                table: "Par_Orario",
                column: "Az_SubCommessaAttivitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_RapportoLavoro_IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_RapportoLavoro",
                column: "IdAz_SubCommessaAttivita");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Timbrature_IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_GG_Timbrature",
                column: "IdAz_SubCommessaAttivita");

            migrationBuilder.AddForeignKey(
                name: "FK_Dip_GG_Timbrature_Az_SubCommessaAttivita_IdAz_SubCommessaAt~",
                schema: "public",
                table: "Dip_GG_Timbrature",
                column: "IdAz_SubCommessaAttivita",
                principalSchema: "public",
                principalTable: "Az_SubCommessaAttivita",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Dip_RapportoLavoro_Az_SubCommessaAttivita_IdAz_SubCommessaA~",
                schema: "public",
                table: "Dip_RapportoLavoro",
                column: "IdAz_SubCommessaAttivita",
                principalSchema: "public",
                principalTable: "Az_SubCommessaAttivita",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_Orario_Az_SubCommessaAttivita_Az_SubCommessaAttivitaId",
                schema: "public",
                table: "Par_Orario",
                column: "Az_SubCommessaAttivitaId",
                principalSchema: "public",
                principalTable: "Az_SubCommessaAttivita",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Par_OrarioIntervalloHH_Az_SubCommessaAttivita_Az_SubCommess~",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                column: "Az_SubCommessaAttivitaId",
                principalSchema: "public",
                principalTable: "Az_SubCommessaAttivita",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dip_GG_Timbrature_Az_SubCommessaAttivita_IdAz_SubCommessaAt~",
                schema: "public",
                table: "Dip_GG_Timbrature");

            migrationBuilder.DropForeignKey(
                name: "FK_Dip_RapportoLavoro_Az_SubCommessaAttivita_IdAz_SubCommessaA~",
                schema: "public",
                table: "Dip_RapportoLavoro");

            migrationBuilder.DropForeignKey(
                name: "FK_Par_Orario_Az_SubCommessaAttivita_Az_SubCommessaAttivitaId",
                schema: "public",
                table: "Par_Orario");

            migrationBuilder.DropForeignKey(
                name: "FK_Par_OrarioIntervalloHH_Az_SubCommessaAttivita_Az_SubCommess~",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropIndex(
                name: "IX_Par_OrarioIntervalloHH_Az_SubCommessaAttivitaId",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropIndex(
                name: "IX_Par_Orario_Az_SubCommessaAttivitaId",
                schema: "public",
                table: "Par_Orario");

            migrationBuilder.DropIndex(
                name: "IX_Dip_RapportoLavoro_IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_RapportoLavoro");

            migrationBuilder.DropIndex(
                name: "IX_Dip_GG_Timbrature_IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_GG_Timbrature");

            migrationBuilder.DropColumn(
                name: "Az_SubCommessaAttivitaId",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "Az_SubCommessaAttivitaId",
                schema: "public",
                table: "Par_Orario");

            migrationBuilder.DropColumn(
                name: "IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_RapportoLavoro");

            migrationBuilder.DropColumn(
                name: "IdAz_SubCommessaAttivita",
                schema: "public",
                table: "Dip_GG_Timbrature");
        }
    }
}
