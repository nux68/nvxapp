using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class AttendanceTracking_TimbraturaReparto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdAz_SediReparto",
                table: "Dip_GG_TimbraturaInfo",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_TimbraturaInfo_IdAz_SediReparto",
                table: "Dip_GG_TimbraturaInfo",
                column: "IdAz_SediReparto");

            migrationBuilder.AddForeignKey(
                name: "FK_Dip_GG_TimbraturaInfo_Az_SediReparto_IdAz_SediReparto",
                table: "Dip_GG_TimbraturaInfo",
                column: "IdAz_SediReparto",
                principalTable: "Az_SediReparto",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dip_GG_TimbraturaInfo_Az_SediReparto_IdAz_SediReparto",
                table: "Dip_GG_TimbraturaInfo");

            migrationBuilder.DropIndex(
                name: "IX_Dip_GG_TimbraturaInfo_IdAz_SediReparto",
                table: "Dip_GG_TimbraturaInfo");

            migrationBuilder.DropColumn(
                name: "IdAz_SediReparto",
                table: "Dip_GG_TimbraturaInfo");
        }
    }
}
