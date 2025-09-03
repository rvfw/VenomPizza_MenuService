using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenomPizzaMenuService.Migrations
{
    /// <inheritdoc />
    public partial class _22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComboProducts_Combos_ComboId",
                table: "ComboProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_ComboProducts_Products_ProductId",
                table: "ComboProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Combos_Products_Id",
                table: "Combos");

            migrationBuilder.DropForeignKey(
                name: "FK_Dishes_Products_Id",
                table: "Dishes");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceVariants_Products_ProductId",
                table: "PriceVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dishes",
                table: "Dishes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Combos",
                table: "Combos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PriceVariants",
                table: "PriceVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComboProducts",
                table: "ComboProducts");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "products");

            migrationBuilder.RenameTable(
                name: "Dishes",
                newName: "dishes");

            migrationBuilder.RenameTable(
                name: "Combos",
                newName: "combos");

            migrationBuilder.RenameTable(
                name: "PriceVariants",
                newName: "price_variants");

            migrationBuilder.RenameTable(
                name: "ComboProducts",
                newName: "combo_products");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "products",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "products",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "products",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Categories",
                table: "products",
                newName: "categories");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "products",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "products",
                newName: "is_available");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "products",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "dishes",
                newName: "unit");

            migrationBuilder.RenameColumn(
                name: "Proteins",
                table: "dishes",
                newName: "proteins");

            migrationBuilder.RenameColumn(
                name: "Fats",
                table: "dishes",
                newName: "fats");

            migrationBuilder.RenameColumn(
                name: "Carbohydrates",
                table: "dishes",
                newName: "carbohydrates");

            migrationBuilder.RenameColumn(
                name: "Calorific",
                table: "dishes",
                newName: "calorific");

            migrationBuilder.RenameColumn(
                name: "Allergens",
                table: "dishes",
                newName: "allergens");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "dishes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Ingredients",
                table: "dishes",
                newName: "ingridients");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "combos",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "price_variants",
                newName: "size");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "price_variants",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "price_variants",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "price_variants",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "IX_PriceVariants_ProductId",
                table: "price_variants",
                newName: "IX_price_variants_product_id");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "combo_products",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "ComboId",
                table: "combo_products",
                newName: "combo_id");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "combo_products",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "IX_ComboProducts_ComboId",
                table: "combo_products",
                newName: "IX_combo_products_combo_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_products",
                table: "products",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dishes",
                table: "dishes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_combos",
                table: "combos",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_price_variants",
                table: "price_variants",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_combo_products",
                table: "combo_products",
                columns: new[] { "product_id", "combo_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_combo_products_combos_combo_id",
                table: "combo_products",
                column: "combo_id",
                principalTable: "combos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_combo_products_products_product_id",
                table: "combo_products",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_combos_products_id",
                table: "combos",
                column: "id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dishes_products_id",
                table: "dishes",
                column: "id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_price_variants_products_product_id",
                table: "price_variants",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_combo_products_combos_combo_id",
                table: "combo_products");

            migrationBuilder.DropForeignKey(
                name: "FK_combo_products_products_product_id",
                table: "combo_products");

            migrationBuilder.DropForeignKey(
                name: "FK_combos_products_id",
                table: "combos");

            migrationBuilder.DropForeignKey(
                name: "FK_dishes_products_id",
                table: "dishes");

            migrationBuilder.DropForeignKey(
                name: "FK_price_variants_products_product_id",
                table: "price_variants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_products",
                table: "products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dishes",
                table: "dishes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_combos",
                table: "combos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_price_variants",
                table: "price_variants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_combo_products",
                table: "combo_products");

            migrationBuilder.RenameTable(
                name: "products",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "dishes",
                newName: "Dishes");

            migrationBuilder.RenameTable(
                name: "combos",
                newName: "Combos");

            migrationBuilder.RenameTable(
                name: "price_variants",
                newName: "PriceVariants");

            migrationBuilder.RenameTable(
                name: "combo_products",
                newName: "ComboProducts");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Products",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "Products",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Products",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "categories",
                table: "Products",
                newName: "Categories");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Products",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "is_available",
                table: "Products",
                newName: "IsAvailable");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "Products",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "unit",
                table: "Dishes",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "proteins",
                table: "Dishes",
                newName: "Proteins");

            migrationBuilder.RenameColumn(
                name: "fats",
                table: "Dishes",
                newName: "Fats");

            migrationBuilder.RenameColumn(
                name: "carbohydrates",
                table: "Dishes",
                newName: "Carbohydrates");

            migrationBuilder.RenameColumn(
                name: "calorific",
                table: "Dishes",
                newName: "Calorific");

            migrationBuilder.RenameColumn(
                name: "allergens",
                table: "Dishes",
                newName: "Allergens");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Dishes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ingridients",
                table: "Dishes",
                newName: "Ingredients");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Combos",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "size",
                table: "PriceVariants",
                newName: "Size");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "PriceVariants",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PriceVariants",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "PriceVariants",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_price_variants_product_id",
                table: "PriceVariants",
                newName: "IX_PriceVariants_ProductId");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "ComboProducts",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "combo_id",
                table: "ComboProducts",
                newName: "ComboId");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "ComboProducts",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_combo_products_combo_id",
                table: "ComboProducts",
                newName: "IX_ComboProducts_ComboId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dishes",
                table: "Dishes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Combos",
                table: "Combos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PriceVariants",
                table: "PriceVariants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComboProducts",
                table: "ComboProducts",
                columns: new[] { "ProductId", "ComboId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ComboProducts_Combos_ComboId",
                table: "ComboProducts",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComboProducts_Products_ProductId",
                table: "ComboProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Combos_Products_Id",
                table: "Combos",
                column: "Id",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Dishes_Products_Id",
                table: "Dishes",
                column: "Id",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceVariants_Products_ProductId",
                table: "PriceVariants",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
