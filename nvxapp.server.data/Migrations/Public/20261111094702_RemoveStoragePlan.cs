using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nvxapp.server.data.Migrations.Public
{
    /// <inheritdoc />
    public partial class RemoveStoragePlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyStorage_StoragePlan_IdStoragePlan",
                schema: "public",
                table: "CompanyStorage");

            migrationBuilder.DropTable(
                name: "StoragePlan",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_CompanyStorage_IdStoragePlan",
                schema: "public",
                table: "CompanyStorage");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "AppSetting",
                keyColumn: "Key",
                keyValue: "Storage_MaxFileMB");

            migrationBuilder.DropColumn(
                name: "IdStoragePlan",
                schema: "public",
                table: "CompanyStorage");

            migrationBuilder.DropColumn(
                name: "MaxFileMB",
                schema: "public",
                table: "CompanyStorage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdStoragePlan",
                schema: "public",
                table: "CompanyStorage",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxFileMB",
                schema: "public",
                table: "CompanyStorage",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StoragePlan",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ChangeUser = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Descrizione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MaxFileMB = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Predefinito = table.Column<bool>(type: "boolean", nullable: false),
                    QuotaMB = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoragePlan", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "AppSetting",
                columns: new[] { "Key", "ModifiedDate", "Value" },
                values: new object[] { "Storage_MaxFileMB", null, "50" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyStorage_IdStoragePlan",
                schema: "public",
                table: "CompanyStorage",
                column: "IdStoragePlan");

            migrationBuilder.CreateIndex(
                name: "IX_StoragePlan_Descrizione",
                schema: "public",
                table: "StoragePlan",
                column: "Descrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoragePlan_Predefinito",
                schema: "public",
                table: "StoragePlan",
                column: "Predefinito",
                unique: true,
                filter: "\"Predefinito\"");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyStorage_StoragePlan_IdStoragePlan",
                schema: "public",
                table: "CompanyStorage",
                column: "IdStoragePlan",
                principalSchema: "public",
                principalTable: "StoragePlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
