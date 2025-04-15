using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adega.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Nome", "Senha", "TipoUsuario" },
                values: new object[] { 1, "admin", "1234", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
