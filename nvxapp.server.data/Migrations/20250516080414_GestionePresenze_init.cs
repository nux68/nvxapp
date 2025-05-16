using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using nvxapp.server.data.Entities.Tenant;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Az_Anagrafica",
                schema: "public",
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
                schema: "public",
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
                name: "Az_Cfg",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_Sedi",
                schema: "public",
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
                    table.PrimaryKey("PK_Az_Sedi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Az_Sedi_Az_Anagrafica_IdAz_Anagrafica",
                        column: x => x.IdAz_Anagrafica,
                        principalSchema: "public",
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_Arrotondamenti",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_Causali",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_Giustificativi",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_Orario",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                        principalSchema: "public",
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_ProfiloOrario",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    Codice = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumGiorniCiclo = table.Column<int>(type: "integer", nullable: false),
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
                        principalSchema: "public",
                        principalTable: "Az_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_RapportoLavoro",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_Anagrafica = table.Column<int>(type: "integer", nullable: false),
                    DataAss = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataLic = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_RapportoLavoro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_RapportoLavoro_Dip_Anagrafica_IdDip_Anagrafica",
                        column: x => x.IdDip_Anagrafica,
                        principalSchema: "public",
                        principalTable: "Dip_Anagrafica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_SediReparto",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_Sedi = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                        principalSchema: "public",
                        principalTable: "Az_SediReparto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Az_SediReparto_Az_Sedi_IdAz_Sedi",
                        column: x => x.IdAz_Sedi,
                        principalSchema: "public",
                        principalTable: "Az_Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_ProfiloOrarioIntervalloHH",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPar_Orario = table.Column<int>(type: "integer", nullable: false),
                    Dalle = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Alle = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ProfiloOrarioIntervalloHH", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloOrarioIntervalloHH_Par_Orario_IdPar_Orario",
                        column: x => x.IdPar_Orario,
                        principalSchema: "public",
                        principalTable: "Par_Orario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_ProfiloOrarioGG",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPar_ProfiloOrario = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Par_ProfiloOrarioGG", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Par_ProfiloOrarioGG_Par_ProfiloOrario_IdPar_ProfiloOrario",
                        column: x => x.IdPar_ProfiloOrario,
                        principalSchema: "public",
                        principalTable: "Par_ProfiloOrario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_Causali",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                        principalSchema: "public",
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Causali_Par_Causali_IdPar_Causali",
                        column: x => x.IdPar_Causali,
                        principalSchema: "public",
                        principalTable: "Par_Causali",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_Richieste",
                schema: "public",
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
                    RichiestaApprovazioneData = table.Column<List<Dip_GG_Richiesta_Stato_Cronology>>(type: "jsonb", nullable: false),
                    RevocaStato = table.Column<int>(type: "integer", nullable: true),
                    RevocaApprovazioneData = table.Column<List<Dip_GG_Richiesta_Stato_Cronology>>(type: "jsonb", nullable: false),
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
                        principalSchema: "public",
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_ProfiloOrario",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Dal = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Al = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                        principalSchema: "public",
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_ProfiloOrario_Par_ProfiloOrario_IdPar_ProfiloOrario",
                        column: x => x.IdPar_ProfiloOrario,
                        principalSchema: "public",
                        principalTable: "Par_ProfiloOrario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_SediRepartoAttivita",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_SediReparto = table.Column<int>(type: "integer", nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                        principalSchema: "public",
                        principalTable: "Az_SediReparto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Az_SediRepartoUser",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAz_SediReparto = table.Column<int>(type: "integer", nullable: false),
                    IdAspNetUsers = table.Column<string>(type: "text", nullable: false),
                    EnabledToApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalZOrder = table.Column<int>(type: "integer", nullable: false),
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
                        principalSchema: "public",
                        principalTable: "Az_SediReparto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Par_OrarioPar_ProfiloOrarioGG",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "Par_Orario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Par_OrarioPar_ProfiloOrarioGG_Par_ProfiloOrarioGG_Par_Profi~",
                        column: x => x.Par_ProfiloOrarioGGId,
                        principalSchema: "public",
                        principalTable: "Par_ProfiloOrarioGG",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_Giustificativi",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "Dip_GG_Richieste",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Giustificativi_Dip_RapportoLavoro_IdDip_RapportoLavo~",
                        column: x => x.IdDip_RapportoLavoro,
                        principalSchema: "public",
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Giustificativi_Par_Giustificativi_IdPar_Giustificati~",
                        column: x => x.IdPar_Giustificativi,
                        principalSchema: "public",
                        principalTable: "Par_Giustificativi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_NotaSpese",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "Dip_GG_Richieste",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_NotaSpese_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalSchema: "public",
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_GG_Timbrature",
                schema: "public",
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
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dip_GG_Timbrature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Timbrature_Dip_GG_Richieste_IdDip_GG_Richiesta",
                        column: x => x.IdDip_GG_Richiesta,
                        principalSchema: "public",
                        principalTable: "Dip_GG_Richieste",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_GG_Timbrature_Dip_RapportoLavoro_IdDip_RapportoLavoro",
                        column: x => x.IdDip_RapportoLavoro,
                        principalSchema: "public",
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Az_Anagrafica_IdCompany",
                schema: "public",
                table: "Az_Anagrafica",
                column: "IdCompany",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Az_Cfg_IdAz_Anagrafica",
                schema: "public",
                table: "Az_Cfg",
                column: "IdAz_Anagrafica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Az_Sedi_IdAz_Anagrafica",
                schema: "public",
                table: "Az_Sedi",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediReparto_IdAz_Sedi",
                schema: "public",
                table: "Az_SediReparto",
                column: "IdAz_Sedi");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediReparto_IdAz_SediReparto",
                schema: "public",
                table: "Az_SediReparto",
                column: "IdAz_SediReparto");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediRepartoAttivita_IdAz_SediReparto",
                schema: "public",
                table: "Az_SediRepartoAttivita",
                column: "IdAz_SediReparto");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediRepartoUser_IdAspNetUsers",
                schema: "public",
                table: "Az_SediRepartoUser",
                column: "IdAspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Az_SediRepartoUser_IdAz_SediReparto",
                schema: "public",
                table: "Az_SediRepartoUser",
                column: "IdAz_SediReparto");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Anagrafica_IdAspNetUsers",
                schema: "public",
                table: "Dip_Anagrafica",
                column: "IdAspNetUsers",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Causali_IdDip_RapportoLavoro",
                schema: "public",
                table: "Dip_GG_Causali",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Causali_IdPar_Causali",
                schema: "public",
                table: "Dip_GG_Causali",
                column: "IdPar_Causali");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Giustificativi_IdDip_GG_Richiesta",
                schema: "public",
                table: "Dip_GG_Giustificativi",
                column: "IdDip_GG_Richiesta");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Giustificativi_IdDip_RapportoLavoro",
                schema: "public",
                table: "Dip_GG_Giustificativi",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Giustificativi_IdPar_Giustificativi",
                schema: "public",
                table: "Dip_GG_Giustificativi",
                column: "IdPar_Giustificativi");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_NotaSpese_IdDip_GG_Richiesta",
                schema: "public",
                table: "Dip_GG_NotaSpese",
                column: "IdDip_GG_Richiesta");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_NotaSpese_IdDip_RapportoLavoro",
                schema: "public",
                table: "Dip_GG_NotaSpese",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Richieste_IdDip_RapportoLavoro",
                schema: "public",
                table: "Dip_GG_Richieste",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Timbrature_IdDip_GG_Richiesta",
                schema: "public",
                table: "Dip_GG_Timbrature",
                column: "IdDip_GG_Richiesta");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Timbrature_IdDip_RapportoLavoro",
                schema: "public",
                table: "Dip_GG_Timbrature",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_ProfiloOrario_IdDip_RapportoLavoro",
                schema: "public",
                table: "Dip_ProfiloOrario",
                column: "IdDip_RapportoLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_ProfiloOrario_IdPar_ProfiloOrario",
                schema: "public",
                table: "Dip_ProfiloOrario",
                column: "IdPar_ProfiloOrario");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_RapportoLavoro_IdDip_Anagrafica",
                schema: "public",
                table: "Dip_RapportoLavoro",
                column: "IdDip_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Arrotondamenti_IdAz_Anagrafica",
                schema: "public",
                table: "Par_Arrotondamenti",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Causali_IdAz_Anagrafica",
                schema: "public",
                table: "Par_Causali",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Giustificativi_IdAz_Anagrafica",
                schema: "public",
                table: "Par_Giustificativi",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_Orario_IdAz_Anagrafica",
                schema: "public",
                table: "Par_Orario",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_OrarioPar_ProfiloOrarioGG_Par_ProfiloOrarioGGId",
                schema: "public",
                table: "Par_OrarioPar_ProfiloOrarioGG",
                column: "Par_ProfiloOrarioGGId");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrario_IdAz_Anagrafica",
                schema: "public",
                table: "Par_ProfiloOrario",
                column: "IdAz_Anagrafica");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrarioGG_IdPar_ProfiloOrario",
                schema: "public",
                table: "Par_ProfiloOrarioGG",
                column: "IdPar_ProfiloOrario");

            migrationBuilder.CreateIndex(
                name: "IX_Par_ProfiloOrarioIntervalloHH_IdPar_Orario",
                schema: "public",
                table: "Par_ProfiloOrarioIntervalloHH",
                column: "IdPar_Orario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Az_Cfg",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Az_SediRepartoAttivita",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Az_SediRepartoUser",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dip_GG_Causali",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dip_GG_Giustificativi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dip_GG_NotaSpese",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dip_GG_Timbrature",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dip_ProfiloOrario",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Par_Arrotondamenti",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Par_OrarioPar_ProfiloOrarioGG",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Par_ProfiloOrarioIntervalloHH",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Az_SediReparto",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Par_Causali",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Par_Giustificativi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dip_GG_Richieste",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Par_ProfiloOrarioGG",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Par_Orario",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Az_Sedi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dip_RapportoLavoro",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Par_ProfiloOrario",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dip_Anagrafica",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Az_Anagrafica",
                schema: "public");
        }
    }
}
