using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class LicenseLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxClienti",
                schema: "public",
                table: "Lic_Piano",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxSpazioMB",
                schema: "public",
                table: "Lic_Piano",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxClientiBloccato",
                schema: "public",
                table: "Lic_Abbonamento",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxSpazioMBBloccato",
                schema: "public",
                table: "Lic_Abbonamento",
                type: "integer",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "public",
                table: "AppSetting",
                columns: new[] { "Key", "ModifiedDate", "Value" },
                values: new object[,]
                {
                    { "Licenze_MaxClienti_Free", null, "100" },
                    { "Licenze_MaxClienti_Pro", null, "" },
                    { "Licenze_MaxClienti_Standard", null, "1000" },
                    { "Licenze_MaxSpazioMB_Free", null, "1024" },
                    { "Licenze_MaxSpazioMB_Pro", null, "20480" },
                    { "Licenze_MaxSpazioMB_Standard", null, "5120" }
                });

            // listini e licenze gia' presenti: limiti predefiniti per livello (ANALISI_LICENZE.md §5.1)
            // CRM e Cumulativo -> clienti (100 / 1000 / illimitati), Documentale e Cumulativo -> spazio (1 / 5 / 20 GB)
            migrationBuilder.Sql("UPDATE public.\"Lic_Piano\" p SET \"MaxClienti\" = CASE p.\"Livello\" WHEN 1 THEN 100 WHEN 2 THEN 1000 ELSE NULL END " +
                                 "FROM public.\"Lic_Listino\" l WHERE l.\"Id\" = p.\"IdListino\" AND l.\"Ambito\" IN (3, 100);");
            migrationBuilder.Sql("UPDATE public.\"Lic_Piano\" p SET \"MaxSpazioMB\" = CASE p.\"Livello\" WHEN 1 THEN 1024 WHEN 2 THEN 5120 ELSE 20480 END " +
                                 "FROM public.\"Lic_Listino\" l WHERE l.\"Id\" = p.\"IdListino\" AND l.\"Ambito\" IN (4, 100);");
            migrationBuilder.Sql("UPDATE public.\"Lic_Abbonamento\" SET \"MaxClientiBloccato\" = CASE \"Livello\" WHEN 1 THEN 100 WHEN 2 THEN 1000 ELSE NULL END " +
                                 "WHERE \"Ambito\" IN (3, 100);");
            migrationBuilder.Sql("UPDATE public.\"Lic_Abbonamento\" SET \"MaxSpazioMBBloccato\" = CASE \"Livello\" WHEN 1 THEN 1024 WHEN 2 THEN 5120 ELSE 20480 END " +
                                 "WHERE \"Ambito\" IN (4, 100);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_MaxClienti_Free");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_MaxClienti_Pro");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_MaxClienti_Standard");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_MaxSpazioMB_Free");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_MaxSpazioMB_Pro");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Licenze_MaxSpazioMB_Standard");

            migrationBuilder.DropColumn(
                name: "MaxClienti",
                schema: "public",
                table: "Lic_Piano");

            migrationBuilder.DropColumn(
                name: "MaxSpazioMB",
                schema: "public",
                table: "Lic_Piano");

            migrationBuilder.DropColumn(
                name: "MaxClientiBloccato",
                schema: "public",
                table: "Lic_Abbonamento");

            migrationBuilder.DropColumn(
                name: "MaxSpazioMBBloccato",
                schema: "public",
                table: "Lic_Abbonamento");
        }
    }
}
