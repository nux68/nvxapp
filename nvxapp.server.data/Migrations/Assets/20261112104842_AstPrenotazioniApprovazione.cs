using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.Assets
{
    /// <inheritdoc />
    public partial class AstPrenotazioniApprovazione : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RichiedeApprovazione",
                table: "Ast_Risorsa",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataDecisione",
                table: "Ast_Prenotazione",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdAspNetUsersDecisione",
                table: "Ast_Prenotazione",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRifiuto",
                table: "Ast_Prenotazione",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RichiestaApprovazione",
                table: "Ast_Prenotazione",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RichiedeApprovazione",
                table: "Ast_Risorsa");

            migrationBuilder.DropColumn(
                name: "DataDecisione",
                table: "Ast_Prenotazione");

            migrationBuilder.DropColumn(
                name: "IdAspNetUsersDecisione",
                table: "Ast_Prenotazione");

            migrationBuilder.DropColumn(
                name: "MotivoRifiuto",
                table: "Ast_Prenotazione");

            migrationBuilder.DropColumn(
                name: "RichiestaApprovazione",
                table: "Ast_Prenotazione");
        }
    }
}
