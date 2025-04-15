using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adega.Migrations
{
    /// <inheritdoc />
    public partial class LicencaComHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashLicenca",
                table: "ConfiguracoesSistema",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "ConfiguracoesSistema",
                columns: new[] { "Id", "DataInstalacao", "HashLicenca", "LicencaValidaAte" },
                values: new object[] { 1, new DateTime(2025, 4, 14, 0, 0, 0, 0, DateTimeKind.Local), "46a91bc588bacdea3d1eeb02995ef47693b446684991d0cf082c01fbefbe8d1b", new DateTime(2025, 5, 14, 0, 0, 0, 0, DateTimeKind.Local) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfiguracoesSistema",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "HashLicenca",
                table: "ConfiguracoesSistema");
        }
    }
}
