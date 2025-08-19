using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenomPizzaMenuService.Migrations
{
    /// <inheritdoc />
    public partial class splitTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComboProduct");

            migrationBuilder.DropColumn(
                name: "Allergens",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Calorific",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Carbohydrates",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Fats",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Ingridients",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Profit",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Proteins",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "PriceVariants",
                table: "Products",
                newName: "Price");

            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Profit = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Combos_Products_Id",
                        column: x => x.Id,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Ingridients = table.Column<List<string>>(type: "text[]", nullable: false),
                    Proteins = table.Column<float>(type: "real", nullable: false),
                    Fats = table.Column<float>(type: "real", nullable: false),
                    Carbohydrates = table.Column<float>(type: "real", nullable: false),
                    Calorific = table.Column<float>(type: "real", nullable: false),
                    Allergens = table.Column<List<string>>(type: "text[]", nullable: false),
                    PriceVariants = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dishes_Products_Id",
                        column: x => x.Id,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Combos_ComboId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Combos");

            migrationBuilder.DropTable(
                name: "Dishes");

            migrationBuilder.DropIndex(
                name: "IX_Products_ComboId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Products",
                newName: "PriceVariants");

            migrationBuilder.AddColumn<List<string>>(
                name: "Allergens",
                table: "Products",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Calorific",
                table: "Products",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Carbohydrates",
                table: "Products",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Fats",
                table: "Products",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Ingridients",
                table: "Products",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductType",
                table: "Products",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Profit",
                table: "Products",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Proteins",
                table: "Products",
                type: "real",
                nullable: true);

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
    }
}
