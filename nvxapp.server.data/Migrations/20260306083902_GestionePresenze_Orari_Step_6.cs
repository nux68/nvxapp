using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SogliaHHStrao",
                schema: "public",
                table: "Par_Orario");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StraoSogliaHHFullTime",
                schema: "public",
                table: "Par_ProfiloOrario",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StraoTipoConteggio",
                schema: "public",
                table: "Par_ProfiloOrario",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StraoSogliaHHFullTime",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropColumn(
                name: "StraoTipoConteggio",
                schema: "public",
                table: "Par_ProfiloOrario");

            migrationBuilder.AddColumn<decimal>(
                name: "SogliaHHStrao",
                schema: "public",
                table: "Par_Orario",
                type: "numeric(4,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
