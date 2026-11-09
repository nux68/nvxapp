using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.DocumentManagement
{
    /// <inheritdoc />
    public partial class Doc_Par_Tipo_TipoInvio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoInvio",
                table: "Doc_Par_Tipo",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoInvio",
                table: "Doc_Par_Tipo");
        }
    }
}
