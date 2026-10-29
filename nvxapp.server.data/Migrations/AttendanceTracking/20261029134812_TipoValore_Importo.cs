using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class TipoValore_Importo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Par_Giustificativi",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Par_Causali",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Importo",
                table: "Dip_GG_Giustificativi",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Importo",
                table: "Dip_GG_Causali",
                type: "numeric(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Par_Giustificativi");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Par_Causali");

            migrationBuilder.DropColumn(
                name: "Importo",
                table: "Dip_GG_Giustificativi");

            migrationBuilder.DropColumn(
                name: "Importo",
                table: "Dip_GG_Causali");
        }
    }
}
