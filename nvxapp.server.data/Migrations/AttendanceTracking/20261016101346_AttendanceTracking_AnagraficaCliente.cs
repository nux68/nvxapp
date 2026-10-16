using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class AttendanceTracking_AnagraficaCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cap",
                table: "Az_Cliente",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Citta",
                table: "Az_Cliente",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodiceDestinatario",
                table: "Az_Cliente",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodiceFiscale",
                table: "Az_Cliente",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Az_Cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Indirizzo",
                table: "Az_Cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nazione",
                table: "Az_Cliente",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Az_Cliente",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartitaIva",
                table: "Az_Cliente",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pec",
                table: "Az_Cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "Az_Cliente",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referente",
                table: "Az_Cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Az_Cliente",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cap",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "Citta",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "CodiceDestinatario",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "CodiceFiscale",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "Indirizzo",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "Nazione",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "PartitaIva",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "Pec",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "Referente",
                table: "Az_Cliente");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Az_Cliente");
        }
    }
}
