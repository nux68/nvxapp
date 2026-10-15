using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nvxapp.server.data.Migrations.AttendanceTracking
{
    /// <inheritdoc />
    public partial class AttendanceTracking_StraoAutorizzato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StraoSoloAutorizzato",
                table: "Par_ProfiloOrario",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutorizzaStraordinario",
                table: "Par_Giustificativi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Min_DaAutorizzare",
                table: "Dip_Periodo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Min_NonAutorizzate",
                table: "Dip_Periodo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Min_DaAutorizzare",
                table: "Dip_GG_Result",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Min_NonAutorizzate",
                table: "Dip_GG_Result",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // aziende gia' esistenti: giustificativo di autorizzazione alle ore in piu' (nelle nuove lo crea il seed)
            migrationBuilder.Sql(@"
                INSERT INTO ""Par_Giustificativi""
                    (""IdAz_Anagrafica"", ""Descrizione"", ""Codice"", ""BackgroundColor"", ""TextColor"", ""TipoInput"", ""IdCausale"",
                     ""Segno"", ""VisualizzaInPianoFerie"", ""TipoContatore"", ""BancaOre"", ""AutorizzaStraordinario"", ""CreationDate"", ""ModifiedDate"")
                SELECT a.""Id"", 'Autorizzazione straordinario', 'AUTS', '#ffc409', '#000000', 0, NULL,
                       0, false, 0, false, true, now(), now()
                FROM ""Az_Anagrafica"" a
                WHERE NOT EXISTS (SELECT 1 FROM ""Par_Giustificativi"" g WHERE g.""IdAz_Anagrafica"" = a.""Id"" AND g.""Codice"" = 'AUTS');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StraoSoloAutorizzato",
                table: "Par_ProfiloOrario");

            migrationBuilder.DropColumn(
                name: "AutorizzaStraordinario",
                table: "Par_Giustificativi");

            migrationBuilder.DropColumn(
                name: "Min_DaAutorizzare",
                table: "Dip_Periodo");

            migrationBuilder.DropColumn(
                name: "Min_NonAutorizzate",
                table: "Dip_Periodo");

            migrationBuilder.DropColumn(
                name: "Min_DaAutorizzare",
                table: "Dip_GG_Result");

            migrationBuilder.DropColumn(
                name: "Min_NonAutorizzate",
                table: "Dip_GG_Result");
        }
    }
}
