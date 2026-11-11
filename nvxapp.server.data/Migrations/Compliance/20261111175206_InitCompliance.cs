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

            migrationBuilder.CreateIndex(
                name: "IX_Cmp_Cfg_IdAz_Anagrafica",
                table: "Cmp_Cfg",
                column: "IdAz_Anagrafica",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cmp_Cfg");
        }
    }
}
