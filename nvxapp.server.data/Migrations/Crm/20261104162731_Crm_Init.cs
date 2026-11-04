using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Crm
{
    /// <inheritdoc />
    public partial class Crm_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Crm_Cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Visibilita = table.Column<int>(type: "integer", nullable: false),
                    Valuta = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MinutiPreavviso = table.Column<int>(type: "integer", nullable: false),
                    OffertaPrefisso = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OffertaAnno = table.Column<int>(type: "integer", nullable: false),
                    OffertaUltimoNumero = table.Column<int>(type: "integer", nullable: false),
                    OffertaGiorniValidita = table.Column<int>(type: "integer", nullable: false),
                    OffertaNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Cfg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Cfg_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Par_Fase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Probabilita = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Par_Fase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Par_Fase_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Par_Fonte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Par_Fonte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Par_Fonte_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Par_MotivoPerdita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Par_MotivoPerdita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Par_MotivoPerdita_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Par_Prodotto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codice = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Prezzo = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Um = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AliquotaIva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Par_Prodotto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Par_Prodotto_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Par_Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Colore = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Par_Tag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Par_Tag_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Par_TipoAttivita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Icona = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Colore = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DurataMinuti = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Par_TipoAttivita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Par_TipoAttivita_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crm_ClienteDati",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    IdCrm_Par_Fonte = table.Column<int>(type: "integer", nullable: true),
                    IdAspNetUsersResponsabile = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Settore = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FatturatoStimato = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    NumeroDipendenti = table.Column<int>(type: "integer", nullable: true),
                    Sito = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConsensoMarketing = table.Column<bool>(type: "boolean", nullable: false),
                    DataConsenso = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_ClienteDati", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_ClienteDati_AspNetUsers_IdAspNetUsersResponsabile",
                        column: x => x.IdAspNetUsersResponsabile,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Crm_ClienteDati_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_ClienteDati_Crm_Par_Fonte_IdCrm_Par_Fonte",
                        column: x => x.IdCrm_Par_Fonte,
                        principalTable: "Crm_Par_Fonte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Lead",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Cognome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Azienda = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Ruolo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IdCrm_Par_Fonte = table.Column<int>(type: "integer", nullable: true),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsersResponsabile = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConsensoMarketing = table.Column<bool>(type: "boolean", nullable: false),
                    DataConsenso = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: true),
                    DataConversione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Lead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Lead_AspNetUsers_IdAspNetUsersResponsabile",
                        column: x => x.IdAspNetUsersResponsabile,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Crm_Lead_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Lead_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Crm_Lead_Crm_Par_Fonte_IdCrm_Par_Fonte",
                        column: x => x.IdCrm_Par_Fonte,
                        principalTable: "Crm_Par_Fonte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Opportunita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: false),
                    IdAz_ClienteContatto = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Par_Fase = table.Column<int>(type: "integer", nullable: false),
                    Valore = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Probabilita = table.Column<int>(type: "integer", nullable: false),
                    DataChiusuraPrevista = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataChiusura = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdAspNetUsersResponsabile = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IdCrm_Par_MotivoPerdita = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Par_Fonte = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IdCommessaPresenze = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Lead = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Opportunita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Opportunita_AspNetUsers_IdAspNetUsersResponsabile",
                        column: x => x.IdAspNetUsersResponsabile,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Crm_Opportunita_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Opportunita_Az_ClienteContatto_IdAz_ClienteContatto",
                        column: x => x.IdAz_ClienteContatto,
                        principalTable: "Az_ClienteContatto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Crm_Opportunita_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Crm_Opportunita_Crm_Par_Fase_IdCrm_Par_Fase",
                        column: x => x.IdCrm_Par_Fase,
                        principalTable: "Crm_Par_Fase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Crm_Opportunita_Crm_Par_Fonte_IdCrm_Par_Fonte",
                        column: x => x.IdCrm_Par_Fonte,
                        principalTable: "Crm_Par_Fonte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Crm_Opportunita_Crm_Par_MotivoPerdita_IdCrm_Par_MotivoPerdi~",
                        column: x => x.IdCrm_Par_MotivoPerdita,
                        principalTable: "Crm_Par_MotivoPerdita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Crm_ClienteTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: false),
                    IdCrm_Par_Tag = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_ClienteTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_ClienteTag_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_ClienteTag_Crm_Par_Tag_IdCrm_Par_Tag",
                        column: x => x.IdCrm_Par_Tag,
                        principalTable: "Crm_Par_Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Attivita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdCrm_Par_TipoAttivita = table.Column<int>(type: "integer", nullable: false),
                    Oggetto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Inizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Fine = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdAspNetUsersAssegnatario = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    Esito = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: true),
                    IdAz_ClienteContatto = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Opportunita = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Lead = table.Column<int>(type: "integer", nullable: true),
                    PromemoriaInviato = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Attivita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Attivita_AspNetUsers_IdAspNetUsersAssegnatario",
                        column: x => x.IdAspNetUsersAssegnatario,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Crm_Attivita_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Attivita_Az_ClienteContatto_IdAz_ClienteContatto",
                        column: x => x.IdAz_ClienteContatto,
                        principalTable: "Az_ClienteContatto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Crm_Attivita_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Crm_Attivita_Crm_Lead_IdCrm_Lead",
                        column: x => x.IdCrm_Lead,
                        principalTable: "Crm_Lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Attivita_Crm_Opportunita_IdCrm_Opportunita",
                        column: x => x.IdCrm_Opportunita,
                        principalTable: "Crm_Opportunita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Attivita_Crm_Par_TipoAttivita_IdCrm_Par_TipoAttivita",
                        column: x => x.IdCrm_Par_TipoAttivita,
                        principalTable: "Crm_Par_TipoAttivita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Nota",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Opportunita = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Lead = table.Column<int>(type: "integer", nullable: true),
                    Testo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IdAspNetUsersAutore = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Fissata = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Nota", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Nota_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Nota_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Nota_Crm_Lead_IdCrm_Lead",
                        column: x => x.IdCrm_Lead,
                        principalTable: "Crm_Lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Nota_Crm_Opportunita_IdCrm_Opportunita",
                        column: x => x.IdCrm_Opportunita,
                        principalTable: "Crm_Opportunita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Offerta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ValidaFinoAl = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: false),
                    IdAz_ClienteContatto = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Opportunita = table.Column<int>(type: "integer", nullable: true),
                    Oggetto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Imponibile = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Iva = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Totale = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataInvio = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Offerta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Offerta_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Offerta_Az_ClienteContatto_IdAz_ClienteContatto",
                        column: x => x.IdAz_ClienteContatto,
                        principalTable: "Az_ClienteContatto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Crm_Offerta_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Crm_Offerta_Crm_Opportunita_IdCrm_Opportunita",
                        column: x => x.IdCrm_Opportunita,
                        principalTable: "Crm_Opportunita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Crm_OpportunitaStorico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCrm_Opportunita = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IdCrm_Par_FaseDa = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Par_FaseA = table.Column<int>(type: "integer", nullable: false),
                    ValoreDa = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ValoreA = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_OpportunitaStorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_OpportunitaStorico_Crm_Opportunita_IdCrm_Opportunita",
                        column: x => x.IdCrm_Opportunita,
                        principalTable: "Crm_Opportunita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crm_Allegato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Opportunita = table.Column<int>(type: "integer", nullable: true),
                    IdCrm_Offerta = table.Column<int>(type: "integer", nullable: true),
                    NomeFile = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Dimensione = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IdAspNetUsersAutore = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_Allegato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_Allegato_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_Allegato_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Crm_Allegato_Crm_Offerta_IdCrm_Offerta",
                        column: x => x.IdCrm_Offerta,
                        principalTable: "Crm_Offerta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Crm_Allegato_Crm_Opportunita_IdCrm_Opportunita",
                        column: x => x.IdCrm_Opportunita,
                        principalTable: "Crm_Opportunita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Crm_OffertaRiga",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCrm_Offerta = table.Column<int>(type: "integer", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    IdCrm_Par_Prodotto = table.Column<int>(type: "integer", nullable: true),
                    Descrizione = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Quantita = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Um = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Prezzo = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ScontoPerc = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    AliquotaIva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Importo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crm_OffertaRiga", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crm_OffertaRiga_Crm_Offerta_IdCrm_Offerta",
                        column: x => x.IdCrm_Offerta,
                        principalTable: "Crm_Offerta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crm_OffertaRiga_Crm_Par_Prodotto_IdCrm_Par_Prodotto",
                        column: x => x.IdCrm_Par_Prodotto,
                        principalTable: "Crm_Par_Prodotto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Allegato_IdAz_Anagrafica",
                table: "Crm_Allegato",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Allegato_IdAz_Cliente",
                table: "Crm_Allegato",
                column: "IdAz_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Allegato_IdCrm_Offerta",
                table: "Crm_Allegato",
                column: "IdCrm_Offerta");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Allegato_IdCrm_Opportunita",
                table: "Crm_Allegato",
                column: "IdCrm_Opportunita");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Attivita_IdAspNetUsersAssegnatario",
                table: "Crm_Attivita",
                column: "IdAspNetUsersAssegnatario");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Attivita_IdAz_Anagrafica_IdAspNetUsersAssegnatario_Stat~",
                table: "Crm_Attivita",
                columns: new[] { "IdAz_Anagrafica", "IdAspNetUsersAssegnatario", "Stato", "Inizio" });

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Attivita_IdAz_Cliente",
                table: "Crm_Attivita",
                column: "IdAz_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Attivita_IdAz_ClienteContatto",
                table: "Crm_Attivita",
                column: "IdAz_ClienteContatto");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Attivita_IdCrm_Lead",
                table: "Crm_Attivita",
                column: "IdCrm_Lead");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Attivita_IdCrm_Opportunita",
                table: "Crm_Attivita",
                column: "IdCrm_Opportunita");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Attivita_IdCrm_Par_TipoAttivita",
                table: "Crm_Attivita",
                column: "IdCrm_Par_TipoAttivita");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Cfg_IdAz_Anagrafica",
                table: "Crm_Cfg",
                column: "IdAz_Anagrafica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crm_ClienteDati_IdAspNetUsersResponsabile",
                table: "Crm_ClienteDati",
                column: "IdAspNetUsersResponsabile");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_ClienteDati_IdAz_Cliente",
                table: "Crm_ClienteDati",
                column: "IdAz_Cliente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crm_ClienteDati_IdCrm_Par_Fonte",
                table: "Crm_ClienteDati",
                column: "IdCrm_Par_Fonte");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_ClienteTag_IdAz_Cliente_IdCrm_Par_Tag",
                table: "Crm_ClienteTag",
                columns: new[] { "IdAz_Cliente", "IdCrm_Par_Tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crm_ClienteTag_IdCrm_Par_Tag",
                table: "Crm_ClienteTag",
                column: "IdCrm_Par_Tag");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Lead_IdAspNetUsersResponsabile",
                table: "Crm_Lead",
                column: "IdAspNetUsersResponsabile");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Lead_IdAz_Anagrafica_Stato",
                table: "Crm_Lead",
                columns: new[] { "IdAz_Anagrafica", "Stato" });

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Lead_IdAz_Cliente",
                table: "Crm_Lead",
                column: "IdAz_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Lead_IdCrm_Par_Fonte",
                table: "Crm_Lead",
                column: "IdCrm_Par_Fonte");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Nota_IdAz_Anagrafica",
                table: "Crm_Nota",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Nota_IdAz_Cliente",
                table: "Crm_Nota",
                column: "IdAz_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Nota_IdCrm_Lead",
                table: "Crm_Nota",
                column: "IdCrm_Lead");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Nota_IdCrm_Opportunita",
                table: "Crm_Nota",
                column: "IdCrm_Opportunita");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Offerta_IdAz_Anagrafica_Anno_Numero",
                table: "Crm_Offerta",
                columns: new[] { "IdAz_Anagrafica", "Anno", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Offerta_IdAz_Cliente",
                table: "Crm_Offerta",
                column: "IdAz_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Offerta_IdAz_ClienteContatto",
                table: "Crm_Offerta",
                column: "IdAz_ClienteContatto");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Offerta_IdCrm_Opportunita",
                table: "Crm_Offerta",
                column: "IdCrm_Opportunita");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_OffertaRiga_IdCrm_Offerta",
                table: "Crm_OffertaRiga",
                column: "IdCrm_Offerta");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_OffertaRiga_IdCrm_Par_Prodotto",
                table: "Crm_OffertaRiga",
                column: "IdCrm_Par_Prodotto");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Opportunita_IdAspNetUsersResponsabile",
                table: "Crm_Opportunita",
                column: "IdAspNetUsersResponsabile");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Opportunita_IdAz_Anagrafica_IdCrm_Par_Fase",
                table: "Crm_Opportunita",
                columns: new[] { "IdAz_Anagrafica", "IdCrm_Par_Fase" });

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Opportunita_IdAz_Cliente",
                table: "Crm_Opportunita",
                column: "IdAz_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Opportunita_IdAz_ClienteContatto",
                table: "Crm_Opportunita",
                column: "IdAz_ClienteContatto");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Opportunita_IdCrm_Par_Fase",
                table: "Crm_Opportunita",
                column: "IdCrm_Par_Fase");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Opportunita_IdCrm_Par_Fonte",
                table: "Crm_Opportunita",
                column: "IdCrm_Par_Fonte");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Opportunita_IdCrm_Par_MotivoPerdita",
                table: "Crm_Opportunita",
                column: "IdCrm_Par_MotivoPerdita");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_OpportunitaStorico_IdCrm_Opportunita",
                table: "Crm_OpportunitaStorico",
                column: "IdCrm_Opportunita");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Par_Fase_IdAz_Anagrafica_Ordine",
                table: "Crm_Par_Fase",
                columns: new[] { "IdAz_Anagrafica", "Ordine" });

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Par_Fonte_IdAz_Anagrafica",
                table: "Crm_Par_Fonte",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Par_MotivoPerdita_IdAz_Anagrafica",
                table: "Crm_Par_MotivoPerdita",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Par_Prodotto_IdAz_Anagrafica_Codice",
                table: "Crm_Par_Prodotto",
                columns: new[] { "IdAz_Anagrafica", "Codice" });

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Par_Tag_IdAz_Anagrafica",
                table: "Crm_Par_Tag",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Crm_Par_TipoAttivita_IdAz_Anagrafica",
                table: "Crm_Par_TipoAttivita",
                column: "IdAz_Anagrafica");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Crm_Allegato");

            migrationBuilder.DropTable(
                name: "Crm_Attivita");

            migrationBuilder.DropTable(
                name: "Crm_Cfg");

            migrationBuilder.DropTable(
                name: "Crm_ClienteDati");

            migrationBuilder.DropTable(
                name: "Crm_ClienteTag");

            migrationBuilder.DropTable(
                name: "Crm_Nota");

            migrationBuilder.DropTable(
                name: "Crm_OffertaRiga");

            migrationBuilder.DropTable(
                name: "Crm_OpportunitaStorico");

            migrationBuilder.DropTable(
                name: "Crm_Par_TipoAttivita");

            migrationBuilder.DropTable(
                name: "Crm_Par_Tag");

            migrationBuilder.DropTable(
                name: "Crm_Lead");

            migrationBuilder.DropTable(
                name: "Crm_Offerta");

            migrationBuilder.DropTable(
                name: "Crm_Par_Prodotto");

            migrationBuilder.DropTable(
                name: "Crm_Opportunita");

            migrationBuilder.DropTable(
                name: "Crm_Par_Fase");

            migrationBuilder.DropTable(
                name: "Crm_Par_Fonte");

            migrationBuilder.DropTable(
                name: "Crm_Par_MotivoPerdita");
        }
    }
}
