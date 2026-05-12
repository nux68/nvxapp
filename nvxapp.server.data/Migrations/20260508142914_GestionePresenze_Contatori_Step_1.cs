using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Contatori_Step_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoContatore",
                schema: "public",
                table: "Par_Giustificativi",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Dip_Contatori_Riporto",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_Contatori_Riporto_Par_Giustificativi_IdPar_Giustificati~",
                        column: x => x.IdPar_Giustificativi,
                        principalSchema: "public",
                        principalTable: "Par_Giustificativi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dip_Rapporto_Giustificativi_Maturazione",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dip_Rapporto_Giustificativi_Maturazione_Par_Giustificativi_~",
                        column: x => x.IdPar_Giustificativi,
                        principalSchema: "public",
                        principalTable: "Par_Giustificativi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Contatori_Riporto_IdDip_RapportoLavoro_IdPar_Giustifica~",
                schema: "public",
                table: "Dip_Contatori_Riporto",
                columns: new[] { "IdDip_RapportoLavoro", "IdPar_Giustificativi", "Anno" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Contatori_Riporto_IdPar_Giustificativi",
                schema: "public",
                table: "Dip_Contatori_Riporto",
                column: "IdPar_Giustificativi");

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Rapporto_Giustificativi_Maturazione_IdDip_RapportoLavor~",
                schema: "public",
                table: "Dip_Rapporto_Giustificativi_Maturazione",
                columns: new[] { "IdDip_RapportoLavoro", "IdPar_Giustificativi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dip_Rapporto_Giustificativi_Maturazione_IdPar_Giustificativi",
                schema: "public",
                table: "Dip_Rapporto_Giustificativi_Maturazione",
                column: "IdPar_Giustificativi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dip_Contatori_Riporto",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dip_Rapporto_Giustificativi_Maturazione",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "TipoContatore",
                schema: "public",
                table: "Par_Giustificativi");
        }
    }
}
