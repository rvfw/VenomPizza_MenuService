using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenomPizzaMenuService.Migrations
{
    /// <inheritdoc />
    public partial class _12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceVariants_Dishes_DishId",
                table: "PriceVariants");

            migrationBuilder.RenameColumn(
                name: "DishId",
                table: "PriceVariants",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_PriceVariants_DishId",
                table: "PriceVariants",
                newName: "IX_PriceVariants_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceVariants_Products_ProductId",
                table: "PriceVariants",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceVariants_Products_ProductId",
                table: "PriceVariants");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "PriceVariants",
                newName: "DishId");

            migrationBuilder.RenameIndex(
                name: "IX_PriceVariants_ProductId",
                table: "PriceVariants",
                newName: "IX_PriceVariants_DishId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceVariants_Dishes_DishId",
                table: "PriceVariants",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
