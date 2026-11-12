using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Common
{
    /// <inheritdoc />
    public partial class InitCommon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Az_Anagrafica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompany = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_Anagrafica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_Anagrafica_Company_IdCompany",
                        column: x => x.IdCompany,
                        principalSchema: "public",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_Cliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    PartitaIva = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    CodiceFiscale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Indirizzo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Cap = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Citta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Provincia = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Nazione = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Pec = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Referente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CodiceDestinatario = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_Cliente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_Cliente_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_Sedi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Default = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_Sedi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_Sedi_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_Anagrafica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "text", nullable: true),
                    Cognome = table.Column<string>(type: "text", nullable: true),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    Matricola = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CodiceFiscale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataAssunzione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataCessazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_Anagrafica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_Anagrafica_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Dip_Anagrafica_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_Competenza",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Default = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_Competenza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_Competenza_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_ClienteContatto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: false),
                    Cognome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ruolo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Cellulare = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Principale = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_ClienteContatto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_ClienteContatto_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_SediReparto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Default = table.Column<bool>(type: "boolean", nullable: false),
                    IdAz_SediReparto = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_SediReparto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_SediReparto_Az_SediReparto_IdAz_SediReparto",
                        column: x => x.IdAz_SediReparto,
                        principalTable: "Az_SediReparto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Az_SediReparto_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_Richiesta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: true),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: true),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataA = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RichiestaTipo = table.Column<int>(type: "integer", nullable: false),
                    Dati = table.Column<string>(type: "text", nullable: false),
                    RichiestaStato = table.Column<int>(type: "integer", nullable: false),
                    RichiestaApprovazioneData = table.Column<string>(type: "text", nullable: false),
                    RevocaStato = table.Column<int>(type: "integer", nullable: true),
                    RevocaApprovazioneData = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_Richiesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Richiesta_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Richiesta_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_RapportoAutovetture",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Targa = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    RimborsoKM = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_RapportoAutovetture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_RapportoAutovetture_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_Competenza",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Competenza = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_Competenza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_Competenza_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_Competenza_Par_Competenza_IdPar_Competenza",
                        column: x => x.IdPar_Competenza,
                        principalTable: "Par_Competenza",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Az_Anagrafica_IdCompany",
                table: "Az_Anagrafica",
                column: "IdCompany",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Az_Cliente_IdAz_Anagrafica",
                table: "Az_Cliente",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Az_ClienteContatto_IdAz_Cliente",
                table: "Az_ClienteContatto",
                column: "IdAz_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Az_Sedi_IdAz_Anagrafica",
                table: "Az_Sedi",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediReparto_IdAz_Sedi",
                table: "Az_SediReparto",
                column: "IdAz_Sedi");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediReparto_IdAz_SediReparto",
                table: "Az_SediReparto",
                column: "IdAz_SediReparto");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Anagrafica_IdAspNetUsers",
                table: "Dip_Anagrafica",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Anagrafica_IdAz_Anagrafica_CodiceFiscale",
                table: "Dip_Anagrafica",
                columns: new[] { "IdAz_Anagrafica", "CodiceFiscale" });

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Anagrafica_IdAz_Anagrafica_IdAspNetUsers",
                table: "Dip_Anagrafica",
                columns: new[] { "IdAz_Anagrafica", "IdAspNetUsers" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Competenza_IdDip_Anagrafica_IdPar_Competenza",
                table: "Dip_Competenza",
                columns: new[] { "IdDip_Anagrafica", "IdPar_Competenza" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Competenza_IdPar_Competenza",
                table: "Dip_Competenza",
                column: "IdPar_Competenza");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Richiesta_IdAspNetUsers",
                table: "Dip_GG_Richiesta",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Richiesta_IdAz_Anagrafica_RichiestaStato",
                table: "Dip_GG_Richiesta",
                columns: new[] { "IdAz_Anagrafica", "RichiestaStato" });

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Richiesta_IdDip_Anagrafica",
                table: "Dip_GG_Richiesta",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Richiesta_IdDip_RapportoLavoro",
                table: "Dip_GG_Richiesta",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_RapportoAutovetture_IdDip_Anagrafica",
                table: "Dip_RapportoAutovetture",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Competenza_IdAz_Anagrafica",
                table: "Par_Competenza",
                column: "IdAz_Anagrafica");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Az_ClienteContatto");

            migrationBuilder.DropTable(
                name: "Az_SediReparto");

            migrationBuilder.DropTable(
                name: "Dip_Competenza");

            migrationBuilder.DropTable(
                name: "Dip_GG_Richiesta");

            migrationBuilder.DropTable(
                name: "Dip_RapportoAutovetture");

            migrationBuilder.DropTable(
                name: "Az_Cliente");

            migrationBuilder.DropTable(
                name: "Az_Sedi");

            migrationBuilder.DropTable(
                name: "Par_Competenza");

            migrationBuilder.DropTable(
                name: "Dip_Anagrafica");

            migrationBuilder.DropTable(
                name: "Az_Anagrafica");
        }
    }
}
