using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenomPizzaMenuService.Migrations
{
    /// <inheritdoc />
    public partial class DeletedComboId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Products_ComboId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ComboId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Costs",
                table: "Products",
                newName: "PriceVariants");

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
                        name: "FK_ComboProduct_Products_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Products",
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

            migrationBuilder.RenameColumn(
                name: "PriceVariants",
                table: "Products",
                newName: "Costs");

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
                name: "FK_Products_Products_ComboId",
                table: "Products",
                column: "ComboId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
