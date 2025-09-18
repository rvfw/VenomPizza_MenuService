using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VenomPizzaMenuService.Migrations
{
    /// <inheritdoc />
    public partial class _11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_price_variants",
                table: "price_variants");

            migrationBuilder.DropIndex(
                name: "IX_price_variants_product_id",
                table: "price_variants");

            migrationBuilder.DropColumn(
                name: "unit",
                table: "dishes");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "price_variants",
                newName: "price_id");

            migrationBuilder.AddColumn<string>(
                name: "unit",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "price_id",
                table: "price_variants",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_price_variants",
                table: "price_variants",
                columns: new[] { "product_id", "price_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_price_variants",
                table: "price_variants");

            migrationBuilder.DropColumn(
                name: "unit",
                table: "products");

            migrationBuilder.RenameColumn(
                name: "price_id",
                table: "price_variants",
                newName: "id");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "price_variants",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "unit",
                table: "dishes",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_price_variants",
                table: "price_variants",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_price_variants_product_id",
                table: "price_variants",
                column: "product_id");
        }
    }
}
