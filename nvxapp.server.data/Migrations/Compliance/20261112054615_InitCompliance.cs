using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Compliance
{
    /// <inheritdoc />
    public partial class InitCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cmp_Cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    GiorniPreavviso = table.Column<int>(type: "integer", nullable: false),
                    NotificheAttive = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UtentiDatiSanitari = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_Cfg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_Cfg_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_Documento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Versione = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DataRevisione = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataScadenza = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Riferimento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_Documento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_Documento_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_Dpi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Norma = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_Dpi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_Dpi_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_Formazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    TipoCorso = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataCorso = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Ore = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: true),
                    Ente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataScadenza = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Attestato = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_Formazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_Formazione_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cmp_Formazione_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_Mansione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Rischio = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_Mansione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_Mansione_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_Nomina",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: true),
                    NominativoEsterno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: true),
                    DataInizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataFine = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Documento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_Nomina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_Nomina_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cmp_Nomina_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cmp_Nomina_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_Responsabile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Denominazione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CodiceFiscale = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Trattamenti = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataContratto = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataScadenza = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Documento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_Responsabile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_Responsabile_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_Trattamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Denominazione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Finalita = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BaseGiuridica = table.Column<int>(type: "integer", nullable: false),
                    CategorieInteressati = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CategorieDati = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DatiParticolari = table.Column<bool>(type: "boolean", nullable: false),
                    Destinatari = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Trasferimenti = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TerminiCancellazione = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MisureSicurezza = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_Trattamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_Trattamento_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_Violazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    DataScoperta = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataViolazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Descrizione = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CategorieDati = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NumeroInteressati = table.Column<int>(type: "integer", nullable: true),
                    Conseguenze = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Misure = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Rischio = table.Column<int>(type: "integer", nullable: false),
                    DataNotificaGarante = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MotivoRitardo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataComunicazioneInteressati = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_Violazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_Violazione_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_Visita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    DataVisita = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Esito = table.Column<int>(type: "integer", nullable: false),
                    Limitazioni = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataScadenza = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Medico = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DatiSanitari = table.Column<string>(type: "text", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_Visita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_Visita_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cmp_Visita_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_DpiConsegna",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdCmp_Dpi = table.Column<int>(type: "integer", nullable: false),
                    DataConsegna = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Quantita = table.Column<int>(type: "integer", nullable: false),
                    Taglia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_DpiConsegna", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_DpiConsegna_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cmp_DpiConsegna_Cmp_Dpi_IdCmp_Dpi",
                        column: x => x.IdCmp_Dpi,
                        principalTable: "Cmp_Dpi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cmp_DpiConsegna_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_DipendenteMansione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdCmp_Mansione = table.Column<int>(type: "integer", nullable: false),
                    DataInizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataFine = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_DipendenteMansione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_DipendenteMansione_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cmp_DipendenteMansione_Cmp_Mansione_IdCmp_Mansione",
                        column: x => x.IdCmp_Mansione,
                        principalTable: "Cmp_Mansione",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cmp_DipendenteMansione_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cmp_MansioneCorso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdCmp_Mansione = table.Column<int>(type: "integer", nullable: false),
                    TipoCorso = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ValiditaMesi = table.Column<int>(type: "integer", nullable: true),
                    Ore = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cmp_MansioneCorso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cmp_MansioneCorso_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cmp_MansioneCorso_Cmp_Mansione_IdCmp_Mansione",
                        column: x => x.IdCmp_Mansione,
                        principalTable: "Cmp_Mansione",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Cfg_IdAz_Anagrafica",
                table: "Cmp_Cfg",
                column: "IdAz_Anagrafica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_DipendenteMansione_IdAz_Anagrafica",
                table: "Cmp_DipendenteMansione",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_DipendenteMansione_IdCmp_Mansione",
                table: "Cmp_DipendenteMansione",
                column: "IdCmp_Mansione");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_DipendenteMansione_IdDip_Anagrafica",
                table: "Cmp_DipendenteMansione",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Documento_IdAz_Anagrafica",
                table: "Cmp_Documento",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Dpi_IdAz_Anagrafica",
                table: "Cmp_Dpi",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_DpiConsegna_IdAz_Anagrafica",
                table: "Cmp_DpiConsegna",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_DpiConsegna_IdCmp_Dpi",
                table: "Cmp_DpiConsegna",
                column: "IdCmp_Dpi");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_DpiConsegna_IdDip_Anagrafica",
                table: "Cmp_DpiConsegna",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Formazione_IdAz_Anagrafica",
                table: "Cmp_Formazione",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Formazione_IdDip_Anagrafica_TipoCorso",
                table: "Cmp_Formazione",
                columns: new[] { "IdDip_Anagrafica", "TipoCorso" });

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Mansione_IdAz_Anagrafica",
                table: "Cmp_Mansione",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_MansioneCorso_IdAz_Anagrafica",
                table: "Cmp_MansioneCorso",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_MansioneCorso_IdCmp_Mansione",
                table: "Cmp_MansioneCorso",
                column: "IdCmp_Mansione");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Nomina_IdAz_Anagrafica",
                table: "Cmp_Nomina",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Nomina_IdAz_Sedi",
                table: "Cmp_Nomina",
                column: "IdAz_Sedi");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Nomina_IdDip_Anagrafica",
                table: "Cmp_Nomina",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Responsabile_IdAz_Anagrafica",
                table: "Cmp_Responsabile",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Trattamento_IdAz_Anagrafica",
                table: "Cmp_Trattamento",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Violazione_IdAz_Anagrafica",
                table: "Cmp_Violazione",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Visita_IdAz_Anagrafica",
                table: "Cmp_Visita",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Visita_IdDip_Anagrafica",
                table: "Cmp_Visita",
                column: "IdDip_Anagrafica");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cmp_Cfg");

            migrationBuilder.DropTable(
                name: "Cmp_DipendenteMansione");

            migrationBuilder.DropTable(
                name: "Cmp_Documento");

            migrationBuilder.DropTable(
                name: "Cmp_DpiConsegna");

            migrationBuilder.DropTable(
                name: "Cmp_Formazione");

            migrationBuilder.DropTable(
                name: "Cmp_MansioneCorso");

            migrationBuilder.DropTable(
                name: "Cmp_Nomina");

            migrationBuilder.DropTable(
                name: "Cmp_Responsabile");

            migrationBuilder.DropTable(
                name: "Cmp_Trattamento");

            migrationBuilder.DropTable(
                name: "Cmp_Violazione");

            migrationBuilder.DropTable(
                name: "Cmp_Visita");

            migrationBuilder.DropTable(
                name: "Cmp_Dpi");

            migrationBuilder.DropTable(
                name: "Cmp_Mansione");
        }
    }
}
