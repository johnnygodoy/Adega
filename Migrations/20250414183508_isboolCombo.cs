using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adega.Migrations
{
    /// <inheritdoc />
    public partial class isboolCombo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComboVirtual",
                table: "Produtos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComboVirtual",
                table: "Produtos");
        }
    }
}
