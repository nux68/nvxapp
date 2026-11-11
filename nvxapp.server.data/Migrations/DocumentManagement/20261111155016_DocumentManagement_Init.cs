using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace nvxapp.server.data.Migrations.DocumentManagement
{
    /// <inheritdoc />
    public partial class DocumentManagement_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Doc_Cartella",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdDoc_Cartella = table.Column<int>(type: "integer", nullable: true),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    VisibileUtenti = table.Column<bool>(type: "boolean", nullable: false),
                    DiSistema = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doc_Cartella", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doc_Cartella_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Doc_Cartella_Doc_Cartella_IdDoc_Cartella",
                        column: x => x.IdDoc_Cartella,
                        principalTable: "Doc_Cartella",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Doc_Cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    GiorniCestino = table.Column<int>(type: "integer", nullable: false),
                    VersioniDaConservare = table.Column<int>(type: "integer", nullable: false),
                    NotificheEmail = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doc_Cfg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doc_Cfg_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doc_Log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDoc_Documento = table.Column<int>(type: "integer", nullable: true),
                    Versione = table.Column<int>(type: "integer", nullable: true),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Azione = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Dettaglio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doc_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Doc_Par_Tipo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    TipoInvio = table.Column<int>(type: "integer", nullable: false),
                    AnniConservazione = table.Column<int>(type: "integer", nullable: true),
                    HaScadenza = table.Column<bool>(type: "boolean", nullable: false),
                    PresaVisioneRichiesta = table.Column<bool>(type: "boolean", nullable: false),
                    FirmaRichiesta = table.Column<bool>(type: "boolean", nullable: false),
                    HaCompetenza = table.Column<bool>(type: "boolean", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doc_Par_Tipo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doc_Par_Tipo_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doc_UtenteAbilitazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    VedePersonale = table.Column<bool>(type: "boolean", nullable: false),
                    CaricaPersonale = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doc_UtenteAbilitazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doc_UtenteAbilitazione_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doc_Documento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdDoc_Par_Tipo = table.Column<int>(type: "integer", nullable: false),
                    IdDoc_Cartella = table.Column<int>(type: "integer", nullable: true),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: true),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: true),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataDocumento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Anno = table.Column<int>(type: "integer", nullable: true),
                    Mese = table.Column<int>(type: "integer", nullable: true),
                    DataScadenza = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    Origine = table.Column<int>(type: "integer", nullable: false),
                    FirmaRichiesta = table.Column<bool>(type: "boolean", nullable: false),
                    NelCestino = table.Column<bool>(type: "boolean", nullable: false),
                    DataCestino = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    VersioneCorrente = table.Column<int>(type: "integer", nullable: false),
                    Testo = table.Column<string>(type: "text", nullable: true),
                    Ricerca = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "italian")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "Titolo", "Descrizione", "Testo" }),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doc_Documento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doc_Documento_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Doc_Documento_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Doc_Documento_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Doc_Documento_Doc_Cartella_IdDoc_Cartella",
                        column: x => x.IdDoc_Cartella,
                        principalTable: "Doc_Cartella",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Doc_Documento_Doc_Par_Tipo_IdDoc_Par_Tipo",
                        column: x => x.IdDoc_Par_Tipo,
                        principalTable: "Doc_Par_Tipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Doc_Destinatario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDoc_Documento = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: true),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    PresaVisioneRichiesta = table.Column<bool>(type: "boolean", nullable: false),
                    DataPrimaApertura = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataPresaVisione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataDownload = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FirmaRichiesta = table.Column<bool>(type: "boolean", nullable: false),
                    DataFirma = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FirmaVersione = table.Column<int>(type: "integer", nullable: true),
                    FirmaHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FirmaIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FirmaUserAgent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IdAspNetUsersFirma = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doc_Destinatario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doc_Destinatario_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Doc_Destinatario_Doc_Documento_IdDoc_Documento",
                        column: x => x.IdDoc_Documento,
                        principalTable: "Doc_Documento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doc_Versione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDoc_Documento = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NomeFile = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Dimensione = table.Column<long>(type: "bigint", nullable: false),
                    Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdAspNetUsersAutore = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doc_Versione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doc_Versione_Doc_Documento_IdDoc_Documento",
                        column: x => x.IdDoc_Documento,
                        principalTable: "Doc_Documento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Cartella_IdAz_Anagrafica",
                table: "Doc_Cartella",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Cartella_IdDoc_Cartella",
                table: "Doc_Cartella",
                column: "IdDoc_Cartella");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Cfg_IdAz_Anagrafica",
                table: "Doc_Cfg",
                column: "IdAz_Anagrafica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Destinatario_IdAspNetUsers",
                table: "Doc_Destinatario",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Destinatario_IdDip_Anagrafica",
                table: "Doc_Destinatario",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Destinatario_IdDoc_Documento",
                table: "Doc_Destinatario",
                column: "IdDoc_Documento");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Documento_DataScadenza",
                table: "Doc_Documento",
                column: "DataScadenza");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Documento_IdAz_Anagrafica_NelCestino_IdDoc_Cartella",
                table: "Doc_Documento",
                columns: new[] { "IdAz_Anagrafica", "NelCestino", "IdDoc_Cartella" });

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Documento_IdAz_Cliente",
                table: "Doc_Documento",
                column: "IdAz_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Documento_IdDip_Anagrafica",
                table: "Doc_Documento",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Documento_IdDoc_Cartella",
                table: "Doc_Documento",
                column: "IdDoc_Cartella");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Documento_IdDoc_Par_Tipo",
                table: "Doc_Documento",
                column: "IdDoc_Par_Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Documento_Ricerca",
                table: "Doc_Documento",
                column: "Ricerca")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Log_Data",
                table: "Doc_Log",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Log_IdDoc_Documento_Data",
                table: "Doc_Log",
                columns: new[] { "IdDoc_Documento", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Par_Tipo_IdAz_Anagrafica_Codice",
                table: "Doc_Par_Tipo",
                columns: new[] { "IdAz_Anagrafica", "Codice" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doc_UtenteAbilitazione_IdAspNetUsers",
                table: "Doc_UtenteAbilitazione",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_UtenteAbilitazione_IdAz_Anagrafica_IdAspNetUsers",
                table: "Doc_UtenteAbilitazione",
                columns: new[] { "IdAz_Anagrafica", "IdAspNetUsers" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Versione_Hash",
                table: "Doc_Versione",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Doc_Versione_IdDoc_Documento_Numero",
                table: "Doc_Versione",
                columns: new[] { "IdDoc_Documento", "Numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Doc_Cfg");

            migrationBuilder.DropTable(
                name: "Doc_Destinatario");

            migrationBuilder.DropTable(
                name: "Doc_Log");

            migrationBuilder.DropTable(
                name: "Doc_UtenteAbilitazione");

            migrationBuilder.DropTable(
                name: "Doc_Versione");

            migrationBuilder.DropTable(
                name: "Doc_Documento");

            migrationBuilder.DropTable(
                name: "Doc_Cartella");

            migrationBuilder.DropTable(
                name: "Doc_Par_Tipo");
        }
    }
}
