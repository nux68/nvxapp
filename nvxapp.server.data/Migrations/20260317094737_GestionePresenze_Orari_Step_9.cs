using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    /// <inheritdoc />
    public partial class GestionePresenze_Orari_Step_9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dip_GG_Result",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDip_RapportoLavoro = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    HH_Teo = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HH_Lav = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Stato = table.Column<int>(type: "integer", nullable: false),
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
                        principalSchema: "public",
                        principalTable: "Dip_RapportoLavoro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dip_GG_Result_IdDip_RapportoLavoro",
                schema: "public",
                table: "Dip_GG_Result",
                column: "IdDip_RapportoLavoro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dip_GG_Result",
                schema: "public");
        }
    }
}
