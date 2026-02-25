using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "Alle_Limite_DX",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Alle_Limite_SX",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Dalle_Limite_DX",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Dalle_Limite_SX",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumCoppia",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumGiorno",
                schema: "public",
                table: "Par_ProfiloOrarioGG",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumeroCoppie",
                schema: "public",
                table: "Par_Orario",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SogliaHHStrao",
                schema: "public",
                table: "Par_Orario",
                type: "numeric(4,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alle_Limite_DX",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "Alle_Limite_SX",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "Dalle_Limite_DX",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "Dalle_Limite_SX",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "NumCoppia",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH");

            migrationBuilder.DropColumn(
                name: "NumGiorno",
                schema: "public",
                table: "Par_ProfiloOrarioGG");

            migrationBuilder.DropColumn(
                name: "NumeroCoppie",
                schema: "public",
                table: "Par_Orario");

            migrationBuilder.DropColumn(
                name: "SogliaHHStrao",
                schema: "public",
                table: "Par_Orario");
        }
    }
}
