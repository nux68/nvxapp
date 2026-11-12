using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Assets
{
    /// <inheritdoc />
    public partial class InitAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ast_Categoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Categoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Categoria_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ast_Cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    GiorniPreavviso = table.Column<int>(type: "integer", nullable: false),
                    NotificheAttive = table.Column<bool>(type: "boolean", nullable: false),
                    PrenotazioneDurataMaxOre = table.Column<int>(type: "integer", nullable: false),
                    PrenotazioneAnticipoMaxGiorni = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Cfg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Cfg_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ast_Veicolo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Targa = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Marca = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Modello = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Alimentazione = table.Column<int>(type: "integer", nullable: false),
                    DataImmatricolazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Possesso = table.Column<int>(type: "integer", nullable: false),
                    Fornitore = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroContratto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ScadenzaContratto = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    KmInclusi = table.Column<int>(type: "integer", nullable: true),
                    CanoneMensile = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: true),
                    Uso = table.Column<int>(type: "integer", nullable: false),
                    CostoKmAci = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    EmissioniCO2 = table.Column<int>(type: "integer", nullable: true),
                    KmAttuali = table.Column<int>(type: "integer", nullable: true),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Veicolo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Veicolo_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_Veicolo_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ast_Bene",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    CodiceInventario = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IdAst_Categoria = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MarcaModello = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroSerie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataAcquisto = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Valore = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    ScadenzaGaranzia = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: true),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Bene", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Bene_Ast_Categoria_IdAst_Categoria",
                        column: x => x.IdAst_Categoria,
                        principalTable: "Ast_Categoria",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ast_Bene_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_Bene_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ast_Multa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAst_Veicolo = table.Column<int>(type: "integer", nullable: false),
                    DataInfrazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    NumeroVerbale = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Luogo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Descrizione = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Importo = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: true),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    DataNotifica = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataPagamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Multa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Multa_Ast_Veicolo_IdAst_Veicolo",
                        column: x => x.IdAst_Veicolo,
                        principalTable: "Ast_Veicolo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_Multa_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ast_Risorsa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: true),
                    Capienza = table.Column<int>(type: "integer", nullable: true),
                    IdAst_Veicolo = table.Column<int>(type: "integer", nullable: true),
                    Attiva = table.Column<bool>(type: "boolean", nullable: false),
                    RichiedeApprovazione = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Risorsa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Risorsa_Ast_Veicolo_IdAst_Veicolo",
                        column: x => x.IdAst_Veicolo,
                        principalTable: "Ast_Veicolo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ast_Risorsa_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_Risorsa_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ast_VeicoloAssegnazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAst_Veicolo = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    DataInizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataFine = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UsoPromiscuo = table.Column<bool>(type: "boolean", nullable: false),
                    PercentualeFringe = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IdDip_RapportoAutovetture = table.Column<int>(type: "integer", nullable: true),
                    DataAvvisoCessazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_VeicoloAssegnazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_VeicoloAssegnazione_Ast_Veicolo_IdAst_Veicolo",
                        column: x => x.IdAst_Veicolo,
                        principalTable: "Ast_Veicolo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_VeicoloAssegnazione_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_VeicoloAssegnazione_Dip_RapportoAutovetture_IdDip_Rappo~",
                        column: x => x.IdDip_RapportoAutovetture,
                        principalTable: "Dip_RapportoAutovetture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ast_VeicoloCosto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAst_Veicolo = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Importo = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Km = table.Column<int>(type: "integer", nullable: true),
                    Descrizione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_VeicoloCosto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_VeicoloCosto_Ast_Veicolo_IdAst_Veicolo",
                        column: x => x.IdAst_Veicolo,
                        principalTable: "Ast_Veicolo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ast_VeicoloScadenza",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAst_Veicolo = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataScadenza = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Km = table.Column<int>(type: "integer", nullable: true),
                    Importo = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    DataEsecuzione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_VeicoloScadenza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_VeicoloScadenza_Ast_Veicolo_IdAst_Veicolo",
                        column: x => x.IdAst_Veicolo,
                        principalTable: "Ast_Veicolo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ast_BeneAssegnazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAst_Bene = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    DataConsegna = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CondizioniConsegna = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataRestituzione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CondizioniRestituzione = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataAvvisoCessazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_BeneAssegnazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_BeneAssegnazione_Ast_Bene_IdAst_Bene",
                        column: x => x.IdAst_Bene,
                        principalTable: "Ast_Bene",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_BeneAssegnazione_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ast_Prenotazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAst_Risorsa = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Inizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Fine = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    DataAnnullamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdDip_GG_Richiesta = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ast_Prenotazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ast_Prenotazione_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ast_Prenotazione_Ast_Risorsa_IdAst_Risorsa",
                        column: x => x.IdAst_Risorsa,
                        principalTable: "Ast_Risorsa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Bene_IdAst_Categoria",
                table: "Ast_Bene",
                column: "IdAst_Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Bene_IdAz_Anagrafica_CodiceInventario",
                table: "Ast_Bene",
                columns: new[] { "IdAz_Anagrafica", "CodiceInventario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Bene_IdAz_Sedi",
                table: "Ast_Bene",
                column: "IdAz_Sedi");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_BeneAssegnazione_Aperta",
                table: "Ast_BeneAssegnazione",
                column: "IdAst_Bene",
                unique: true,
                filter: "\"DataRestituzione\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_BeneAssegnazione_IdAst_Bene_DataConsegna",
                table: "Ast_BeneAssegnazione",
                columns: new[] { "IdAst_Bene", "DataConsegna" });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_BeneAssegnazione_IdDip_Anagrafica",
                table: "Ast_BeneAssegnazione",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Categoria_IdAz_Anagrafica_Descrizione",
                table: "Ast_Categoria",
                columns: new[] { "IdAz_Anagrafica", "Descrizione" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Cfg_IdAz_Anagrafica",
                table: "Ast_Cfg",
                column: "IdAz_Anagrafica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Multa_IdAst_Veicolo_DataInfrazione",
                table: "Ast_Multa",
                columns: new[] { "IdAst_Veicolo", "DataInfrazione" });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Multa_IdDip_Anagrafica",
                table: "Ast_Multa",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Prenotazione_IdAspNetUsers_Inizio",
                table: "Ast_Prenotazione",
                columns: new[] { "IdAspNetUsers", "Inizio" });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Prenotazione_IdAst_Risorsa_Inizio_Fine",
                table: "Ast_Prenotazione",
                columns: new[] { "IdAst_Risorsa", "Inizio", "Fine" });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Risorsa_IdAst_Veicolo",
                table: "Ast_Risorsa",
                column: "IdAst_Veicolo");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Risorsa_IdAz_Anagrafica_Tipo_Descrizione",
                table: "Ast_Risorsa",
                columns: new[] { "IdAz_Anagrafica", "Tipo", "Descrizione" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Risorsa_IdAz_Sedi",
                table: "Ast_Risorsa",
                column: "IdAz_Sedi");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Veicolo_IdAz_Anagrafica_Targa",
                table: "Ast_Veicolo",
                columns: new[] { "IdAz_Anagrafica", "Targa" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ast_Veicolo_IdAz_Sedi",
                table: "Ast_Veicolo",
                column: "IdAz_Sedi");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_VeicoloAssegnazione_IdAst_Veicolo_DataInizio",
                table: "Ast_VeicoloAssegnazione",
                columns: new[] { "IdAst_Veicolo", "DataInizio" });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_VeicoloAssegnazione_IdDip_Anagrafica",
                table: "Ast_VeicoloAssegnazione",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_VeicoloAssegnazione_IdDip_RapportoAutovetture",
                table: "Ast_VeicoloAssegnazione",
                column: "IdDip_RapportoAutovetture");

            migrationBuilder.CreateIndex(
                name: "IX_Ast_VeicoloCosto_IdAst_Veicolo_Data",
                table: "Ast_VeicoloCosto",
                columns: new[] { "IdAst_Veicolo", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_Ast_VeicoloScadenza_IdAst_Veicolo_DataScadenza",
                table: "Ast_VeicoloScadenza",
                columns: new[] { "IdAst_Veicolo", "DataScadenza" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ast_BeneAssegnazione");

            migrationBuilder.DropTable(
                name: "Ast_Cfg");

            migrationBuilder.DropTable(
                name: "Ast_Multa");

            migrationBuilder.DropTable(
                name: "Ast_Prenotazione");

            migrationBuilder.DropTable(
                name: "Ast_VeicoloAssegnazione");

            migrationBuilder.DropTable(
                name: "Ast_VeicoloCosto");

            migrationBuilder.DropTable(
                name: "Ast_VeicoloScadenza");

            migrationBuilder.DropTable(
                name: "Ast_Bene");

            migrationBuilder.DropTable(
                name: "Ast_Risorsa");

            migrationBuilder.DropTable(
                name: "Ast_Categoria");

            migrationBuilder.DropTable(
                name: "Ast_Veicolo");
        }
    }
}
