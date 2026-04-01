using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Alle_Use_4_Match",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Dalle_Use_4_Match",
                schema: "public",
                table: "Par_OrarioIntervalloHH",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alle_Use_4_Match",
                schema: "public",
                table: "Par_OrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "Dalle_Use_4_Match",
                schema: "public",
                table: "Par_OrarioIntervalloHH");
        }
    }
}
