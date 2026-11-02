using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class TicketAiUseParentDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseParentDocuments",
                schema: "public",
                table: "TicketTypeAiSetting",
                type: "boolean",
                nullable: false,
                defaultValue: true);   // righe esistenti: documentazione dei livelli sopra usata, come prima
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseParentDocuments",
                schema: "public",
                table: "TicketTypeAiSetting");
        }
    }
}
