using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_PianoFerie_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VisualizzaInPianoFerie",
                schema: "public",
                table: "Par_Giustificativi",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisualizzaInPianoFerie",
                schema: "public",
                table: "Par_Giustificativi");
        }
    }
}
