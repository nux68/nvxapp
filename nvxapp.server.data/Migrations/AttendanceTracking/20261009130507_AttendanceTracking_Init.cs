using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class AttendanceTracking_Init : Migration
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
                name: "Dip_Anagrafica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAspNetUsers = table.Column<string>(type: "text", nullable: false),
                    Cognome = table.Column<string>(type: "text", nullable: true),
                    Nome = table.Column<string>(type: "text", nullable: true),
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "My_template1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_My_template1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Az_Cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    ApprovazioneTipo = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_Cfg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_Cfg_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
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
                    Default = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "Par_Arrotondamenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Dalle = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Alle = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ArrotondamentoTipo = table.Column<int>(type: "integer", nullable: true),
                    ArrotondamentoValore = table.Column<int>(type: "integer", nullable: true),
                    ArrotondamentoVerso = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_Arrotondamenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_Arrotondamenti_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_Attivita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BackgroundColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    TextColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Default = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_Attivita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_Attivita_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_Causali",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_Causali", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_Causali_Az_Anagrafica_IdAz_Anagrafica",
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
                name: "Par_ExportCau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TipoFile = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ExportCau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ExportCau_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_Commessa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdAz_Cliente = table.Column<int>(type: "integer", nullable: false),
                    Default = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_Commessa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_Commessa_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Az_Commessa_Az_Cliente_IdAz_Cliente",
                        column: x => x.IdAz_Cliente,
                        principalTable: "Az_Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "Az_SediAttivita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Attivita = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_SediAttivita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_SediAttivita_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Az_SediAttivita_Par_Attivita_IdPar_Attivita",
                        column: x => x.IdPar_Attivita,
                        principalTable: "Par_Attivita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_Giustificativi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BackgroundColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    TextColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    TipoInput = table.Column<int>(type: "integer", nullable: false),
                    IdCausale = table.Column<int>(type: "integer", nullable: true),
                    Segno = table.Column<int>(type: "integer", nullable: false),
                    VisualizzaInPianoFerie = table.Column<bool>(type: "boolean", nullable: false),
                    TipoContatore = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_Giustificativi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_Giustificativi_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Par_Giustificativi_Par_Causali_IdCausale",
                        column: x => x.IdCausale,
                        principalTable: "Par_Causali",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "Par_AttivitaCompetenza",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPar_Attivita = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Competenza = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_AttivitaCompetenza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_AttivitaCompetenza_Par_Attivita_IdPar_Attivita",
                        column: x => x.IdPar_Attivita,
                        principalTable: "Par_Attivita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Par_AttivitaCompetenza_Par_Competenza_IdPar_Competenza",
                        column: x => x.IdPar_Competenza,
                        principalTable: "Par_Competenza",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_ExportCau_Causali",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPar_ExportCau = table.Column<int>(type: "integer", nullable: false),
                    IdCausale = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TipoElaborazione = table.Column<int>(type: "integer", nullable: false),
                    TipoUnita = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ExportCau_Causali", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ExportCau_Causali_Par_Causali_IdCausale",
                        column: x => x.IdCausale,
                        principalTable: "Par_Causali",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Par_ExportCau_Causali_Par_ExportCau_IdPar_ExportCau",
                        column: x => x.IdPar_ExportCau,
                        principalTable: "Par_ExportCau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_SubCommessa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Commessa = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Default = table.Column<bool>(type: "boolean", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataA = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_SubCommessa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_SubCommessa_Az_Commessa_IdAz_Commessa",
                        column: x => x.IdAz_Commessa,
                        principalTable: "Az_Commessa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_SediRepartoAttivita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_SediReparto = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Attivita = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_SediRepartoAttivita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_SediRepartoAttivita_Az_SediReparto_IdAz_SediReparto",
                        column: x => x.IdAz_SediReparto,
                        principalTable: "Az_SediReparto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Az_SediRepartoAttivita_Par_Attivita_IdPar_Attivita",
                        column: x => x.IdPar_Attivita,
                        principalTable: "Par_Attivita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_SediRepartoUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_SediReparto = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "text", nullable: false),
                    EnabledToAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    EnabledToApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalZOrder = table.Column<int>(type: "integer", nullable: false),
                    UserInDepartment = table.Column<bool>(type: "boolean", nullable: false),
                    DataDal = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataAl = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_SediRepartoUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_SediRepartoUser_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Az_SediRepartoUser_Az_SediReparto_IdAz_SediReparto",
                        column: x => x.IdAz_SediReparto,
                        principalTable: "Az_SediReparto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_SubCommessaAttivita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_SubCommessa = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Attivita = table.Column<int>(type: "integer", nullable: false),
                    Default = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_SubCommessaAttivita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_SubCommessaAttivita_Az_SubCommessa_IdAz_SubCommessa",
                        column: x => x.IdAz_SubCommessa,
                        principalTable: "Az_SubCommessa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Az_SubCommessaAttivita_Par_Attivita_IdPar_Attivita",
                        column: x => x.IdPar_Attivita,
                        principalTable: "Par_Attivita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_SubCommessaSediReparto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_SubCommessa = table.Column<int>(type: "integer", nullable: false),
                    IdAz_SediReparto = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_SubCommessaSediReparto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_SubCommessaSediReparto_Az_SediReparto_IdAz_SediReparto",
                        column: x => x.IdAz_SediReparto,
                        principalTable: "Az_SediReparto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Az_SubCommessaSediReparto_Az_SubCommessa_IdAz_SubCommessa",
                        column: x => x.IdAz_SubCommessa,
                        principalTable: "Az_SubCommessa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_SubCommessaUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_SubCommessa = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Az_SubCommessaUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_SubCommessaUser_AspNetUsers_IdAspNetUsers",
                        column: x => x.IdAspNetUsers,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Az_SubCommessaUser_Az_SubCommessa_IdAz_SubCommessa",
                        column: x => x.IdAz_SubCommessa,
                        principalTable: "Az_SubCommessa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_RapportoLavoro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    DataAss = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataLic = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdAz_SubCommessaAttivita = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_RapportoLavoro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_RapportoLavoro_Az_SubCommessaAttivita_IdAz_SubCommessaA~",
                        column: x => x.IdAz_SubCommessaAttivita,
                        principalTable: "Az_SubCommessaAttivita",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Dip_RapportoLavoro_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_Orario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumeroCoppie = table.Column<int>(type: "integer", nullable: false),
                    IdCausale_HH_Lav_MonteOre = table.Column<int>(type: "integer", nullable: true),
                    TimbratureTipo = table.Column<int>(type: "integer", nullable: false),
                    Hh_Teo_MonteOre = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Az_SubCommessaAttivitaId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_Orario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_Orario_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Par_Orario_Az_SubCommessaAttivita_Az_SubCommessaAttivitaId",
                        column: x => x.Az_SubCommessaAttivitaId,
                        principalTable: "Az_SubCommessaAttivita",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Par_Orario_Par_Causali_IdCausale_HH_Lav_MonteOre",
                        column: x => x.IdCausale_HH_Lav_MonteOre,
                        principalTable: "Par_Causali",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Dip_Contatori_Riporto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Giustificativi = table.Column<int>(type: "integer", nullable: false),
                    Anno = table.Column<int>(type: "integer", nullable: false),
                    SaldoRiporto = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsManuale = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_Contatori_Riporto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_Contatori_Riporto_Dip_RapportoLavoro_IdDip_RapportoLavo~",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_Contatori_Riporto_Par_Giustificativi_IdPar_Giustificati~",
                        column: x => x.IdPar_Giustificativi,
                        principalTable: "Par_Giustificativi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_Causali",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Valore = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IdPar_Causali = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_Causali", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Causali_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Causali_Par_Causali_IdPar_Causali",
                        column: x => x.IdPar_Causali,
                        principalTable: "Par_Causali",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_Result",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    HH_Teo = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HH_Lav = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Stato = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_Result", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Result_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_Richieste",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataA = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RichiestaTipo = table.Column<int>(type: "integer", nullable: false),
                    Dati = table.Column<string>(type: "text", nullable: false),
                    RichiestaStato = table.Column<int>(type: "integer", nullable: false),
                    RichiestaApprovazioneData = table.Column<string>(type: "jsonb", nullable: false),
                    RevocaStato = table.Column<int>(type: "integer", nullable: true),
                    RevocaApprovazioneData = table.Column<string>(type: "jsonb", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_Richieste", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Richieste_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_Rapporto_Giustificativi_Maturazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Giustificativi = table.Column<int>(type: "integer", nullable: false),
                    OreMaturazione = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_Rapporto_Giustificativi_Maturazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_Rapporto_Giustificativi_Maturazione_Dip_RapportoLavoro_~",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_Rapporto_Giustificativi_Maturazione_Par_Giustificativi_~",
                        column: x => x.IdPar_Giustificativi,
                        principalTable: "Par_Giustificativi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_OrarioIntervalloHH",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPar_Orario = table.Column<int>(type: "integer", nullable: false),
                    Dalle = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Dalle_Limite_SX = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Dalle_Limite_DX = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Dalle_Arrotondamento = table.Column<int>(type: "integer", nullable: false),
                    Dalle_Arrotondamento_Verso = table.Column<int>(type: "integer", nullable: false),
                    Dalle_Use_4_Match = table.Column<bool>(type: "boolean", nullable: false),
                    Alle = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Alle_Limite_SX = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Alle_Limite_DX = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Alle_Arrotondamento = table.Column<int>(type: "integer", nullable: false),
                    Alle_Arrotondamento_Verso = table.Column<int>(type: "integer", nullable: false),
                    Alle_Use_4_Match = table.Column<bool>(type: "boolean", nullable: false),
                    NumCoppia = table.Column<int>(type: "integer", nullable: false),
                    IdCausale_HH_Lav = table.Column<int>(type: "integer", nullable: false),
                    Az_SubCommessaAttivitaId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_OrarioIntervalloHH", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_OrarioIntervalloHH_Az_SubCommessaAttivita_Az_SubCommess~",
                        column: x => x.Az_SubCommessaAttivitaId,
                        principalTable: "Az_SubCommessaAttivita",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Par_OrarioIntervalloHH_Par_Causali_IdCausale_HH_Lav",
                        column: x => x.IdCausale_HH_Lav,
                        principalTable: "Par_Causali",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Par_OrarioIntervalloHH_Par_Orario_IdPar_Orario",
                        column: x => x.IdPar_Orario,
                        principalTable: "Par_Orario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_ProfiloOrario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumGiorniCiclo = table.Column<int>(type: "integer", nullable: false),
                    TipoProfilo = table.Column<int>(type: "integer", nullable: false),
                    StraoSogliaHHFullTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    StraoTipoConteggio = table.Column<int>(type: "integer", nullable: false),
                    SupplTipoConteggio = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Orario_Festivo = table.Column<int>(type: "integer", nullable: false),
                    IdCausale_Lavoro_Strao = table.Column<int>(type: "integer", nullable: false),
                    IdCausale_Lavoro_Suppl = table.Column<int>(type: "integer", nullable: false),
                    IdGiustificativo_Assenza_Ingiust = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ProfiloOrario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloOrario_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloOrario_Par_Causali_IdCausale_Lavoro_Strao",
                        column: x => x.IdCausale_Lavoro_Strao,
                        principalTable: "Par_Causali",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloOrario_Par_Causali_IdCausale_Lavoro_Suppl",
                        column: x => x.IdCausale_Lavoro_Suppl,
                        principalTable: "Par_Causali",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloOrario_Par_Giustificativi_IdGiustificativo_Assen~",
                        column: x => x.IdGiustificativo_Assenza_Ingiust,
                        principalTable: "Par_Giustificativi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloOrario_Par_Orario_IdPar_Orario_Festivo",
                        column: x => x.IdPar_Orario_Festivo,
                        principalTable: "Par_Orario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_Giustificativi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IdJustificationType = table.Column<int>(type: "integer", nullable: false),
                    InputType = table.Column<int>(type: "integer", nullable: false),
                    Hours = table.Column<TimeSpan>(type: "interval", nullable: true),
                    From = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IdPar_Giustificativi = table.Column<int>(type: "integer", nullable: false),
                    RichiestaStato = table.Column<int>(type: "integer", nullable: false),
                    IdDip_GG_Richiesta = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_Giustificativi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Giustificativi_Dip_GG_Richieste_IdDip_GG_Richiesta",
                        column: x => x.IdDip_GG_Richiesta,
                        principalTable: "Dip_GG_Richieste",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Giustificativi_Dip_RapportoLavoro_IdDip_RapportoLavo~",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Giustificativi_Par_Giustificativi_IdPar_Giustificati~",
                        column: x => x.IdPar_Giustificativi,
                        principalTable: "Par_Giustificativi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_NotaSpese",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RichiestaStato = table.Column<int>(type: "integer", nullable: false),
                    IdDip_GG_Richiesta = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_NotaSpese", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_NotaSpese_Dip_GG_Richieste_IdDip_GG_Richiesta",
                        column: x => x.IdDip_GG_Richiesta,
                        principalTable: "Dip_GG_Richieste",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_NotaSpese_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_Timbrature",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Timbratura = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimbraturaOriginale = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimbraturaArrotondata = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GiornoCompetenza = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimbraturaTipo = table.Column<int>(type: "integer", nullable: false),
                    RichiestaStato = table.Column<int>(type: "integer", nullable: false),
                    IdDip_GG_Richiesta = table.Column<int>(type: "integer", nullable: true),
                    IdAz_SubCommessaAttivita = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_Timbrature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Timbrature_Az_SubCommessaAttivita_IdAz_SubCommessaAt~",
                        column: x => x.IdAz_SubCommessaAttivita,
                        principalTable: "Az_SubCommessaAttivita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Timbrature_Dip_GG_Richieste_IdDip_GG_Richiesta",
                        column: x => x.IdDip_GG_Richiesta,
                        principalTable: "Dip_GG_Richieste",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Timbrature_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_ProfiloOrario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Dal = table.Column<DateTime>(type: "date", nullable: false),
                    Al = table.Column<DateTime>(type: "date", nullable: false),
                    IdPar_ProfiloOrario = table.Column<int>(type: "integer", nullable: true),
                    NumGiornoPartenzaCiclo = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_ProfiloOrario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_ProfiloOrario_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_ProfiloOrario_Par_ProfiloOrario_IdPar_ProfiloOrario",
                        column: x => x.IdPar_ProfiloOrario,
                        principalTable: "Par_ProfiloOrario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_ProfiloOrarioGG",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPar_ProfiloOrario = table.Column<int>(type: "integer", nullable: false),
                    NumGiorno = table.Column<int>(type: "integer", nullable: false),
                    ZOrder = table.Column<int>(type: "integer", nullable: false),
                    IdPar_Orario = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ProfiloOrarioGG", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloOrarioGG_Par_Orario_IdPar_Orario",
                        column: x => x.IdPar_Orario,
                        principalTable: "Par_Orario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloOrarioGG_Par_ProfiloOrario_IdPar_ProfiloOrario",
                        column: x => x.IdPar_ProfiloOrario,
                        principalTable: "Par_ProfiloOrario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_OrarioPar_ProfiloOrarioGG",
                columns: table => new
                {
                    Par_OrarioId = table.Column<int>(type: "integer", nullable: false),
                    Par_ProfiloOrarioGGId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_OrarioPar_ProfiloOrarioGG", x => new { x.Par_OrarioId, x.Par_ProfiloOrarioGGId });
                    table.ForeignKey(
                        name: "FK_Par_OrarioPar_ProfiloOrarioGG_Par_Orario_Par_OrarioId",
                        column: x => x.Par_OrarioId,
                        principalTable: "Par_Orario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Par_OrarioPar_ProfiloOrarioGG_Par_ProfiloOrarioGG_Par_Profi~",
                        column: x => x.Par_ProfiloOrarioGGId,
                        principalTable: "Par_ProfiloOrarioGG",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Az_Anagrafica_IdCompany",
                table: "Az_Anagrafica",
                column: "IdCompany",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Az_Cfg_IdAz_Anagrafica",
                table: "Az_Cfg",
                column: "IdAz_Anagrafica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Az_Cliente_IdAz_Anagrafica",
                table: "Az_Cliente",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Az_Commessa_IdAz_Anagrafica",
                table: "Az_Commessa",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Az_Commessa_IdAz_Cliente",
                table: "Az_Commessa",
                column: "IdAz_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Az_Sedi_IdAz_Anagrafica",
                table: "Az_Sedi",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediAttivita_IdAz_Sedi",
                table: "Az_SediAttivita",
                column: "IdAz_Sedi");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediAttivita_IdPar_Attivita",
                table: "Az_SediAttivita",
                column: "IdPar_Attivita");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediReparto_IdAz_Sedi",
                table: "Az_SediReparto",
                column: "IdAz_Sedi");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediReparto_IdAz_SediReparto",
                table: "Az_SediReparto",
                column: "IdAz_SediReparto");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediRepartoAttivita_IdAz_SediReparto_IdPar_Attivita",
                table: "Az_SediRepartoAttivita",
                columns: new[] { "IdAz_SediReparto", "IdPar_Attivita" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediRepartoAttivita_IdPar_Attivita",
                table: "Az_SediRepartoAttivita",
                column: "IdPar_Attivita");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediRepartoUser_IdAspNetUsers",
                table: "Az_SediRepartoUser",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediRepartoUser_IdAz_SediReparto_IdAspNetUsers",
                table: "Az_SediRepartoUser",
                columns: new[] { "IdAz_SediReparto", "IdAspNetUsers" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Az_SubCommessa_IdAz_Commessa",
                table: "Az_SubCommessa",
                column: "IdAz_Commessa");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SubCommessaAttivita_IdAz_SubCommessa",
                table: "Az_SubCommessaAttivita",
                column: "IdAz_SubCommessa");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SubCommessaAttivita_IdPar_Attivita",
                table: "Az_SubCommessaAttivita",
                column: "IdPar_Attivita");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SubCommessaSediReparto_IdAz_SediReparto",
                table: "Az_SubCommessaSediReparto",
                column: "IdAz_SediReparto");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SubCommessaSediReparto_IdAz_SubCommessa_IdAz_SediReparto",
                table: "Az_SubCommessaSediReparto",
                columns: new[] { "IdAz_SubCommessa", "IdAz_SediReparto" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Az_SubCommessaUser_IdAspNetUsers",
                table: "Az_SubCommessaUser",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SubCommessaUser_IdAz_SubCommessa_IdAspNetUsers",
                table: "Az_SubCommessaUser",
                columns: new[] { "IdAz_SubCommessa", "IdAspNetUsers" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Anagrafica_IdAspNetUsers",
                table: "Dip_Anagrafica",
                column: "IdAspNetUsers",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Competenza_IdDip_Anagrafica",
                table: "Dip_Competenza",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Competenza_IdPar_Competenza",
                table: "Dip_Competenza",
                column: "IdPar_Competenza");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Contatori_Riporto_IdDip_RapportoLavoro_IdPar_Giustifica~",
                table: "Dip_Contatori_Riporto",
                columns: new[] { "IdDip_RapportoLavoro", "IdPar_Giustificativi", "Anno" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Contatori_Riporto_IdPar_Giustificativi",
                table: "Dip_Contatori_Riporto",
                column: "IdPar_Giustificativi");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Causali_IdDip_RapportoLavoro",
                table: "Dip_GG_Causali",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Causali_IdPar_Causali",
                table: "Dip_GG_Causali",
                column: "IdPar_Causali");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Giustificativi_IdDip_GG_Richiesta",
                table: "Dip_GG_Giustificativi",
                column: "IdDip_GG_Richiesta");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Giustificativi_IdDip_RapportoLavoro",
                table: "Dip_GG_Giustificativi",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Giustificativi_IdPar_Giustificativi",
                table: "Dip_GG_Giustificativi",
                column: "IdPar_Giustificativi");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_NotaSpese_IdDip_GG_Richiesta",
                table: "Dip_GG_NotaSpese",
                column: "IdDip_GG_Richiesta");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_NotaSpese_IdDip_RapportoLavoro",
                table: "Dip_GG_NotaSpese",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Result_IdDip_RapportoLavoro",
                table: "Dip_GG_Result",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Richieste_IdDip_RapportoLavoro",
                table: "Dip_GG_Richieste",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Timbrature_IdAz_SubCommessaAttivita",
                table: "Dip_GG_Timbrature",
                column: "IdAz_SubCommessaAttivita");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Timbrature_IdDip_GG_Richiesta",
                table: "Dip_GG_Timbrature",
                column: "IdDip_GG_Richiesta");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Timbrature_IdDip_RapportoLavoro",
                table: "Dip_GG_Timbrature",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_ProfiloOrario_IdDip_RapportoLavoro",
                table: "Dip_ProfiloOrario",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_ProfiloOrario_IdPar_ProfiloOrario",
                table: "Dip_ProfiloOrario",
                column: "IdPar_ProfiloOrario");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Rapporto_Giustificativi_Maturazione_IdDip_RapportoLavor~",
                table: "Dip_Rapporto_Giustificativi_Maturazione",
                columns: new[] { "IdDip_RapportoLavoro", "IdPar_Giustificativi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Rapporto_Giustificativi_Maturazione_IdPar_Giustificativi",
                table: "Dip_Rapporto_Giustificativi_Maturazione",
                column: "IdPar_Giustificativi");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_RapportoLavoro_IdAz_SubCommessaAttivita",
                table: "Dip_RapportoLavoro",
                column: "IdAz_SubCommessaAttivita");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_RapportoLavoro_IdDip_Anagrafica",
                table: "Dip_RapportoLavoro",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Arrotondamenti_IdAz_Anagrafica",
                table: "Par_Arrotondamenti",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Attivita_IdAz_Anagrafica",
                table: "Par_Attivita",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_AttivitaCompetenza_IdPar_Attivita",
                table: "Par_AttivitaCompetenza",
                column: "IdPar_Attivita");

            migrationBuilder.CreateIndex(
                name: "IX_Par_AttivitaCompetenza_IdPar_Competenza",
                table: "Par_AttivitaCompetenza",
                column: "IdPar_Competenza");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Causali_IdAz_Anagrafica",
                table: "Par_Causali",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Competenza_IdAz_Anagrafica",
                table: "Par_Competenza",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ExportCau_IdAz_Anagrafica",
                table: "Par_ExportCau",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ExportCau_Causali_IdCausale",
                table: "Par_ExportCau_Causali",
                column: "IdCausale");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ExportCau_Causali_IdPar_ExportCau",
                table: "Par_ExportCau_Causali",
                column: "IdPar_ExportCau");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Giustificativi_IdAz_Anagrafica",
                table: "Par_Giustificativi",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Giustificativi_IdCausale",
                table: "Par_Giustificativi",
                column: "IdCausale");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Orario_Az_SubCommessaAttivitaId",
                table: "Par_Orario",
                column: "Az_SubCommessaAttivitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Orario_IdAz_Anagrafica",
                table: "Par_Orario",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Orario_IdCausale_HH_Lav_MonteOre",
                table: "Par_Orario",
                column: "IdCausale_HH_Lav_MonteOre");

            migrationBuilder.CreateIndex(
                name: "IX_Par_OrarioIntervalloHH_Az_SubCommessaAttivitaId",
                table: "Par_OrarioIntervalloHH",
                column: "Az_SubCommessaAttivitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Par_OrarioIntervalloHH_IdCausale_HH_Lav",
                table: "Par_OrarioIntervalloHH",
                column: "IdCausale_HH_Lav");

            migrationBuilder.CreateIndex(
                name: "IX_Par_OrarioIntervalloHH_IdPar_Orario",
                table: "Par_OrarioIntervalloHH",
                column: "IdPar_Orario");

            migrationBuilder.CreateIndex(
                name: "IX_Par_OrarioPar_ProfiloOrarioGG_Par_ProfiloOrarioGGId",
                table: "Par_OrarioPar_ProfiloOrarioGG",
                column: "Par_ProfiloOrarioGGId");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrario_IdAz_Anagrafica",
                table: "Par_ProfiloOrario",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrario_IdCausale_Lavoro_Strao",
                table: "Par_ProfiloOrario",
                column: "IdCausale_Lavoro_Strao");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrario_IdCausale_Lavoro_Suppl",
                table: "Par_ProfiloOrario",
                column: "IdCausale_Lavoro_Suppl");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrario_IdGiustificativo_Assenza_Ingiust",
                table: "Par_ProfiloOrario",
                column: "IdGiustificativo_Assenza_Ingiust");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrario_IdPar_Orario_Festivo",
                table: "Par_ProfiloOrario",
                column: "IdPar_Orario_Festivo");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrarioGG_IdPar_Orario",
                table: "Par_ProfiloOrarioGG",
                column: "IdPar_Orario");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrarioGG_IdPar_ProfiloOrario",
                table: "Par_ProfiloOrarioGG",
                column: "IdPar_ProfiloOrario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Az_Cfg");

            migrationBuilder.DropTable(
                name: "Az_SediAttivita");

            migrationBuilder.DropTable(
                name: "Az_SediRepartoAttivita");

            migrationBuilder.DropTable(
                name: "Az_SediRepartoUser");

            migrationBuilder.DropTable(
                name: "Az_SubCommessaSediReparto");

            migrationBuilder.DropTable(
                name: "Az_SubCommessaUser");

            migrationBuilder.DropTable(
                name: "Dip_Competenza");

            migrationBuilder.DropTable(
                name: "Dip_Contatori_Riporto");

            migrationBuilder.DropTable(
                name: "Dip_GG_Causali");

            migrationBuilder.DropTable(
                name: "Dip_GG_Giustificativi");

            migrationBuilder.DropTable(
                name: "Dip_GG_NotaSpese");

            migrationBuilder.DropTable(
                name: "Dip_GG_Result");

            migrationBuilder.DropTable(
                name: "Dip_GG_Timbrature");

            migrationBuilder.DropTable(
                name: "Dip_ProfiloOrario");

            migrationBuilder.DropTable(
                name: "Dip_Rapporto_Giustificativi_Maturazione");

            migrationBuilder.DropTable(
                name: "My_template1");

            migrationBuilder.DropTable(
                name: "Par_Arrotondamenti");

            migrationBuilder.DropTable(
                name: "Par_AttivitaCompetenza");

            migrationBuilder.DropTable(
                name: "Par_ExportCau_Causali");

            migrationBuilder.DropTable(
                name: "Par_OrarioIntervalloHH");

            migrationBuilder.DropTable(
                name: "Par_OrarioPar_ProfiloOrarioGG");

            migrationBuilder.DropTable(
                name: "Az_SediReparto");

            migrationBuilder.DropTable(
                name: "Dip_GG_Richieste");

            migrationBuilder.DropTable(
                name: "Par_Competenza");

            migrationBuilder.DropTable(
                name: "Par_ExportCau");

            migrationBuilder.DropTable(
                name: "Par_ProfiloOrarioGG");

            migrationBuilder.DropTable(
                name: "Az_Sedi");

            migrationBuilder.DropTable(
                name: "Dip_RapportoLavoro");

            migrationBuilder.DropTable(
                name: "Par_ProfiloOrario");

            migrationBuilder.DropTable(
                name: "Dip_Anagrafica");

            migrationBuilder.DropTable(
                name: "Par_Giustificativi");

            migrationBuilder.DropTable(
                name: "Par_Orario");

            migrationBuilder.DropTable(
                name: "Az_SubCommessaAttivita");

            migrationBuilder.DropTable(
                name: "Par_Causali");

            migrationBuilder.DropTable(
                name: "Az_SubCommessa");

            migrationBuilder.DropTable(
                name: "Par_Attivita");

            migrationBuilder.DropTable(
                name: "Az_Commessa");

            migrationBuilder.DropTable(
                name: "Az_Cliente");

            migrationBuilder.DropTable(
                name: "Az_Anagrafica");
        }
    }
}
