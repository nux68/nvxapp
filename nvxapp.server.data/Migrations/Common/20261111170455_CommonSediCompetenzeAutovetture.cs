using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Common
{
    /// <inheritdoc />
    public partial class CommonSediCompetenzeAutovetture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAssunzione",
                table: "Dip_Anagrafica",
                type: "timestamp without time zone",
                nullable: true);

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
                name: "IX_Dip_Competenza_IdDip_Anagrafica_IdPar_Competenza",
                table: "Dip_Competenza",
                columns: new[] { "IdDip_Anagrafica", "IdPar_Competenza" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Competenza_IdPar_Competenza",
                table: "Dip_Competenza",
                column: "IdPar_Competenza");

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
                name: "Az_SediReparto");

            migrationBuilder.DropTable(
                name: "Dip_Competenza");

            migrationBuilder.DropTable(
                name: "Dip_RapportoAutovetture");

            migrationBuilder.DropTable(
                name: "Az_Sedi");

            migrationBuilder.DropTable(
                name: "Par_Competenza");

            migrationBuilder.DropColumn(
                name: "DataAssunzione",
                table: "Dip_Anagrafica");
        }
    }
}
