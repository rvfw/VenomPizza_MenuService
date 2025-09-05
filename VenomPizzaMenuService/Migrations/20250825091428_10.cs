using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenomPizzaMenuService.Migrations
{
    /// <inheritdoc />
    public partial class _10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceVariant_Dishes_DishId",
                table: "PriceVariant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PriceVariant",
                table: "PriceVariant");

            migrationBuilder.RenameTable(
                name: "PriceVariant",
                newName: "PriceVariants");

            migrationBuilder.RenameIndex(
                name: "IX_PriceVariant_DishId",
                table: "PriceVariants",
                newName: "IX_PriceVariants_DishId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PriceVariants",
                table: "PriceVariants",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceVariants_Dishes_DishId",
                table: "PriceVariants",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceVariants_Dishes_DishId",
                table: "PriceVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PriceVariants",
                table: "PriceVariants");

            migrationBuilder.RenameTable(
                name: "PriceVariants",
                newName: "PriceVariant");

            migrationBuilder.RenameIndex(
                name: "IX_PriceVariants_DishId",
                table: "PriceVariant",
                newName: "IX_PriceVariant_DishId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PriceVariant",
                table: "PriceVariant",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceVariant_Dishes_DishId",
                table: "PriceVariant",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
