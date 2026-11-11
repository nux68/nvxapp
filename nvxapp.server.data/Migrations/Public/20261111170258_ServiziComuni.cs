using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class ServiziComuni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifica",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IdCompany = table.Column<int>(type: "integer", nullable: true),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Messaggio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UrlState = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Letta = table.Column<bool>(type: "boolean", nullable: false),
                    DataLettura = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataEmail = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifica_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifica_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificaPreferenza",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    App = table.Column<bool>(type: "boolean", nullable: false),
                    Email = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificaPreferenza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificaPreferenza_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PresaVisione",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    Application = table.Column<int>(type: "integer", nullable: false),
                    Fonte = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdRiferimento = table.Column<int>(type: "integer", nullable: true),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Testo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UrlState = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Canale = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdCanale = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataConferma = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Annullata = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresaVisione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresaVisione_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PresaVisione_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scadenza",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    Application = table.Column<int>(type: "integer", nullable: false),
                    Fonte = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdRiferimento = table.Column<int>(type: "integer", nullable: true),
                    Descrizione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataScadenza = table.Column<DateTime>(type: "date", nullable: false),
                    GiorniPreavviso = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsersResponsabile = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UrlState = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    DataChiusura = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataAvvisoPreavviso = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataAvvisoScaduta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scadenza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scadenza_AspNetUsers_IdAspNetUsersResponsabile",
                        column: x => x.IdAspNetUsersResponsabile,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Scadenza_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifica_IdAspNetUsers_Letta",
                schema: "public",
                table: "Notifica",
                columns: new[] { "IdAspNetUsers", "Letta" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifica_IdCompany",
                schema: "public",
                table: "Notifica",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_NotificaPreferenza_IdAspNetUsers_Categoria",
                schema: "public",
                table: "NotificaPreferenza",
                columns: new[] { "IdAspNetUsers", "Categoria" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PresaVisione_IdAspNetUsers_DataConferma",
                schema: "public",
                table: "PresaVisione",
                columns: new[] { "IdAspNetUsers", "DataConferma" });

            migrationBuilder.CreateIndex(
                name: "IX_PresaVisione_IdCompany_Application_Fonte_IdRiferimento_IdAs~",
                schema: "public",
                table: "PresaVisione",
                columns: new[] { "IdCompany", "Application", "Fonte", "IdRiferimento", "IdAspNetUsers" },
                unique: true,
                filter: "\"IdRiferimento\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Scadenza_IdAspNetUsersResponsabile",
                schema: "public",
                table: "Scadenza",
                column: "IdAspNetUsersResponsabile");

            migrationBuilder.CreateIndex(
                name: "IX_Scadenza_IdCompany_Application_Fonte_IdRiferimento",
                schema: "public",
                table: "Scadenza",
                columns: new[] { "IdCompany", "Application", "Fonte", "IdRiferimento" },
                unique: true,
                filter: "\"IdRiferimento\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Scadenza_Stato_DataScadenza",
                schema: "public",
                table: "Scadenza",
                columns: new[] { "Stato", "DataScadenza" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifica",
                schema: "public");

            migrationBuilder.DropTable(
                name: "NotificaPreferenza",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PresaVisione",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Scadenza",
                schema: "public");
        }
    }
}
