using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenomPizzaMenuService.Migrations
{
    /// <inheritdoc />
    public partial class tpc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Combos_ComboId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ComboId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "ComboProduct",
                columns: table => new
                {
                    ComboId = table.Column<int>(type: "integer", nullable: false),
                    ProductsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboProduct", x => new { x.ComboId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ComboProduct_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComboProduct_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComboProduct_ProductsId",
                table: "ComboProduct",
                column: "ProductsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComboProduct");

            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ComboId",
                table: "Products",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Combos_ComboId",
                table: "Products",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "Id");
        }
    }
}
