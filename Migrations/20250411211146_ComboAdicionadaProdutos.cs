using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adega.Migrations
{
    /// <inheritdoc />
    public partial class ComboAdicionadaProdutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProdutoId",
                table: "Combos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Combos_ProdutoId",
                table: "Combos",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Combos_Produtos_ProdutoId",
                table: "Combos",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Combos_Produtos_ProdutoId",
                table: "Combos");

            migrationBuilder.DropIndex(
                name: "IX_Combos_ProdutoId",
                table: "Combos");

            migrationBuilder.DropColumn(
                name: "ProdutoId",
                table: "Combos");
        }
    }
}
