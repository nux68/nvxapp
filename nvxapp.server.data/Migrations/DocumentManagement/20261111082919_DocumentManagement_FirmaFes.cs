using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.DocumentManagement
{
    /// <inheritdoc />
    public partial class DocumentManagement_FirmaFes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FirmaRichiesta",
                table: "Doc_Par_Tipo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FirmaRichiesta",
                table: "Doc_Documento",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFirma",
                table: "Doc_Destinatario",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirmaHash",
                table: "Doc_Destinatario",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirmaIp",
                table: "Doc_Destinatario",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FirmaRichiesta",
                table: "Doc_Destinatario",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FirmaUserAgent",
                table: "Doc_Destinatario",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FirmaVersione",
                table: "Doc_Destinatario",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdAspNetUsersFirma",
                table: "Doc_Destinatario",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirmaRichiesta",
                table: "Doc_Par_Tipo");

            migrationBuilder.DropColumn(
                name: "FirmaRichiesta",
                table: "Doc_Documento");

            migrationBuilder.DropColumn(
                name: "DataFirma",
                table: "Doc_Destinatario");

            migrationBuilder.DropColumn(
                name: "FirmaHash",
                table: "Doc_Destinatario");

            migrationBuilder.DropColumn(
                name: "FirmaIp",
                table: "Doc_Destinatario");

            migrationBuilder.DropColumn(
                name: "FirmaRichiesta",
                table: "Doc_Destinatario");

            migrationBuilder.DropColumn(
                name: "FirmaUserAgent",
                table: "Doc_Destinatario");

            migrationBuilder.DropColumn(
                name: "FirmaVersione",
                table: "Doc_Destinatario");

            migrationBuilder.DropColumn(
                name: "IdAspNetUsersFirma",
                table: "Doc_Destinatario");
        }
    }
}
