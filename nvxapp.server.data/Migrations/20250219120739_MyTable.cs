using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using nvxapp.server.data.Infrastructure;

#nullable disable

namespace nvxapp.server.data.Migrations
{
    public partial class MyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            if (SharedSchema.MultiTenant == true && SharedSchema.CurrentSchema == "public")
                return;

            migrationBuilder.EnsureSchema(
                name: SharedSchema.CurrentSchema);

            migrationBuilder.CreateTable(
                name: "MyTable",
                schema: SharedSchema.CurrentSchema,
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyTable", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MyTable_Descrizione",
                schema: SharedSchema.CurrentSchema,
                table: "MyTable",
                column: "Descrizione",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (SharedSchema.MultiTenant == true && SharedSchema.CurrentSchema == "public")
                return;

            migrationBuilder.DropTable(
                name: "MyTable",
                schema: SharedSchema.CurrentSchema);
        }
    }
}
