using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adega.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarBancoDeDadoPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ConfiguracoesSistema",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DataInstalacao", "HashLicenca", "LicencaValidaAte" },
                values: new object[] { new DateTime(2025, 8, 8, 0, 0, 0, 0, DateTimeKind.Local), "d10699f344e05c2b06ab643fcc1c1b8e405aacadc85963074057e4f030a36b53", new DateTime(2025, 9, 7, 0, 0, 0, 0, DateTimeKind.Local) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ConfiguracoesSistema",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DataInstalacao", "HashLicenca", "LicencaValidaAte" },
                values: new object[] { new DateTime(2025, 4, 16, 0, 0, 0, 0, DateTimeKind.Local), "f6326575ffaae6429d7fff31105f76900b294e258f90e0511dfde786fa53001e", new DateTime(2025, 5, 16, 0, 0, 0, 0, DateTimeKind.Local) });
        }
    }
}
