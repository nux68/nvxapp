using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class AttendanceTracking_TurniMultipli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dip_Turno_IdDip_RapportoLavoro_Data",
                table: "Dip_Turno");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Turno_IdDip_RapportoLavoro_Data_IdPar_Turno",
                table: "Dip_Turno",
                columns: new[] { "IdDip_RapportoLavoro", "Data", "IdPar_Turno" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dip_Turno_IdDip_RapportoLavoro_Data_IdPar_Turno",
                table: "Dip_Turno");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Turno_IdDip_RapportoLavoro_Data",
                table: "Dip_Turno",
                columns: new[] { "IdDip_RapportoLavoro", "Data" },
                unique: true);
        }
    }
}
