using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.HumanResources
{
    /// <inheritdoc />
    public partial class InitHumanResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hr_Cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    GiorniPreavviso = table.Column<int>(type: "integer", nullable: false),
                    NotificheAttive = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_Cfg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_Cfg_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hr_Checklist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdHr_ChecklistModello = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataRiferimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_Checklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_Checklist_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hr_Checklist_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Hr_ChecklistModello",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Predefinito = table.Column<bool>(type: "boolean", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_ChecklistModello", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_ChecklistModello_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hr_Corso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DurataOre = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    ValiditaMesi = table.Column<int>(type: "integer", nullable: true),
                    IdPar_Competenza = table.Column<int>(type: "integer", nullable: true),
                    Obbligatorio = table.Column<bool>(type: "boolean", nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_Corso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_Corso_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hr_Corso_Par_Competenza_IdPar_Competenza",
                        column: x => x.IdPar_Competenza,
                        principalTable: "Par_Competenza",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Hr_DocCaricamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    Mese = table.Column<int>(type: "integer", nullable: true),
                    Origine = table.Column<int>(type: "integer", nullable: false),
                    IdFinancialAdvisor = table.Column<int>(type: "integer", nullable: true),
                    IdAspNetUsersCaricamento = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    NomeFile = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Dimensione = table.Column<long>(type: "bigint", nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
                    NumeroPagine = table.Column<int>(type: "integer", nullable: false),
                    Messaggio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataPubblicazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_DocCaricamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_DocCaricamento_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hr_PianoFormazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_PianoFormazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_PianoFormazione_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hr_ChecklistVoce",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdHr_Checklist = table.Column<int>(type: "integer", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IdAspNetUsersResponsabile = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Giorni = table.Column<int>(type: "integer", nullable: false),
                    DataPrevista = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Completata = table.Column<bool>(type: "boolean", nullable: false),
                    DataCompletamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdAspNetUsersCompletamento = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Documento = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentoNomeFile = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    DocumentoContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentoStorageKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentoDimensione = table.Column<long>(type: "bigint", nullable: true),
                    DataRichiestaPresaVisione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Azione = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_ChecklistVoce", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_ChecklistVoce_Hr_Checklist_IdHr_Checklist",
                        column: x => x.IdHr_Checklist,
                        principalTable: "Hr_Checklist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hr_ChecklistModelloVoce",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdHr_ChecklistModello = table.Column<int>(type: "integer", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IdAspNetUsersResponsabile = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Giorni = table.Column<int>(type: "integer", nullable: false),
                    Documento = table.Column<bool>(type: "boolean", nullable: false),
                    Azione = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_ChecklistModelloVoce", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_ChecklistModelloVoce_Hr_ChecklistModello_IdHr_ChecklistM~",
                        column: x => x.IdHr_ChecklistModello,
                        principalTable: "Hr_ChecklistModello",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hr_CorsoEdizione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdHr_Corso = table.Column<int>(type: "integer", nullable: false),
                    DataInizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataFine = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: true),
                    Luogo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Docente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_CorsoEdizione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_CorsoEdizione_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Hr_CorsoEdizione_Hr_Corso_IdHr_Corso",
                        column: x => x.IdHr_Corso,
                        principalTable: "Hr_Corso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hr_DocDipendente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    IdHr_DocCaricamento = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    Mese = table.Column<int>(type: "integer", nullable: true),
                    Pagine = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Dimensione = table.Column<long>(type: "bigint", nullable: false),
                    Metodo = table.Column<int>(type: "integer", nullable: false),
                    DataPubblicazione = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_DocDipendente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_DocDipendente_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hr_DocDipendente_Hr_DocCaricamento_IdHr_DocCaricamento",
                        column: x => x.IdHr_DocCaricamento,
                        principalTable: "Hr_DocCaricamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hr_DocScarto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdHr_DocCaricamento = table.Column<int>(type: "integer", nullable: false),
                    Pagina = table.Column<int>(type: "integer", nullable: false),
                    Testo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IdDip_AnagraficaProposta = table.Column<int>(type: "integer", nullable: true),
                    Motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Esito = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_DocScarto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_DocScarto_Hr_DocCaricamento_IdHr_DocCaricamento",
                        column: x => x.IdHr_DocCaricamento,
                        principalTable: "Hr_DocCaricamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hr_PianoFormazioneRiga",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdHr_PianoFormazione = table.Column<int>(type: "integer", nullable: false),
                    IdHr_Corso = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    EntroIl = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_PianoFormazioneRiga", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_PianoFormazioneRiga_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hr_PianoFormazioneRiga_Hr_Corso_IdHr_Corso",
                        column: x => x.IdHr_Corso,
                        principalTable: "Hr_Corso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hr_PianoFormazioneRiga_Hr_PianoFormazione_IdHr_PianoFormazi~",
                        column: x => x.IdHr_PianoFormazione,
                        principalTable: "Hr_PianoFormazione",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hr_CorsoPartecipante",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdHr_CorsoEdizione = table.Column<int>(type: "integer", nullable: false),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Esito = table.Column<int>(type: "integer", nullable: false),
                    DataConseguimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataScadenza = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AttestatoNomeFile = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    AttestatoContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AttestatoStorageKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AttestatoDimensione = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hr_CorsoPartecipante", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hr_CorsoPartecipante_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hr_CorsoPartecipante_Hr_CorsoEdizione_IdHr_CorsoEdizione",
                        column: x => x.IdHr_CorsoEdizione,
                        principalTable: "Hr_CorsoEdizione",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hr_Cfg_IdAz_Anagrafica",
                table: "Hr_Cfg",
                column: "IdAz_Anagrafica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hr_Checklist_IdAz_Anagrafica",
                table: "Hr_Checklist",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_Checklist_IdDip_Anagrafica_Tipo",
                table: "Hr_Checklist",
                columns: new[] { "IdDip_Anagrafica", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_Hr_ChecklistModello_IdAz_Anagrafica",
                table: "Hr_ChecklistModello",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_ChecklistModelloVoce_IdHr_ChecklistModello",
                table: "Hr_ChecklistModelloVoce",
                column: "IdHr_ChecklistModello");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_ChecklistVoce_IdHr_Checklist",
                table: "Hr_ChecklistVoce",
                column: "IdHr_Checklist");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_Corso_IdAz_Anagrafica",
                table: "Hr_Corso",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_Corso_IdPar_Competenza",
                table: "Hr_Corso",
                column: "IdPar_Competenza");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_CorsoEdizione_IdAz_Sedi",
                table: "Hr_CorsoEdizione",
                column: "IdAz_Sedi");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_CorsoEdizione_IdHr_Corso",
                table: "Hr_CorsoEdizione",
                column: "IdHr_Corso");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_CorsoPartecipante_IdDip_Anagrafica",
                table: "Hr_CorsoPartecipante",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_CorsoPartecipante_IdHr_CorsoEdizione_IdDip_Anagrafica",
                table: "Hr_CorsoPartecipante",
                columns: new[] { "IdHr_CorsoEdizione", "IdDip_Anagrafica" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hr_DocCaricamento_IdAz_Anagrafica_Anno_Mese",
                table: "Hr_DocCaricamento",
                columns: new[] { "IdAz_Anagrafica", "Anno", "Mese" });

            migrationBuilder.CreateIndex(
                name: "IX_Hr_DocDipendente_IdDip_Anagrafica_DataPubblicazione",
                table: "Hr_DocDipendente",
                columns: new[] { "IdDip_Anagrafica", "DataPubblicazione" });

            migrationBuilder.CreateIndex(
                name: "IX_Hr_DocDipendente_IdHr_DocCaricamento_IdDip_Anagrafica",
                table: "Hr_DocDipendente",
                columns: new[] { "IdHr_DocCaricamento", "IdDip_Anagrafica" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hr_DocScarto_IdHr_DocCaricamento_Pagina",
                table: "Hr_DocScarto",
                columns: new[] { "IdHr_DocCaricamento", "Pagina" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hr_PianoFormazione_IdAz_Anagrafica_Anno",
                table: "Hr_PianoFormazione",
                columns: new[] { "IdAz_Anagrafica", "Anno" });

            migrationBuilder.CreateIndex(
                name: "IX_Hr_PianoFormazioneRiga_IdDip_Anagrafica",
                table: "Hr_PianoFormazioneRiga",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_PianoFormazioneRiga_IdHr_Corso",
                table: "Hr_PianoFormazioneRiga",
                column: "IdHr_Corso");

            migrationBuilder.CreateIndex(
                name: "IX_Hr_PianoFormazioneRiga_IdHr_PianoFormazione",
                table: "Hr_PianoFormazioneRiga",
                column: "IdHr_PianoFormazione");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hr_Cfg");

            migrationBuilder.DropTable(
                name: "Hr_ChecklistModelloVoce");

            migrationBuilder.DropTable(
                name: "Hr_ChecklistVoce");

            migrationBuilder.DropTable(
                name: "Hr_CorsoPartecipante");

            migrationBuilder.DropTable(
                name: "Hr_DocDipendente");

            migrationBuilder.DropTable(
                name: "Hr_DocScarto");

            migrationBuilder.DropTable(
                name: "Hr_PianoFormazioneRiga");

            migrationBuilder.DropTable(
                name: "Hr_ChecklistModello");

            migrationBuilder.DropTable(
                name: "Hr_Checklist");

            migrationBuilder.DropTable(
                name: "Hr_CorsoEdizione");

            migrationBuilder.DropTable(
                name: "Hr_DocCaricamento");

            migrationBuilder.DropTable(
                name: "Hr_PianoFormazione");

            migrationBuilder.DropTable(
                name: "Hr_Corso");
        }
    }
}
