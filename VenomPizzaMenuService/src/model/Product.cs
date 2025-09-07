
using System.ComponentModel.DataAnnotations.Schema;
using VenomPizzaMenuService.src.dto;

namespace VenomPizzaMenuService.src.model;
[Table("products")]
public class Product
{
    [Column("id")]
    public int Id { get; init; }
    [Column("title")]
    public string Title { get; set; }
    [Column("image_url")]
    public string? ImageUrl { get; set; }
    [Column("description")]
    public string? Description { get; set; }
    [Column("price")]
    public decimal Price { get; set; }
    [Column("is_available")]
    public bool IsAvailable { get; set; }
    [Column("categories")]
    public List<string> Categories { get; set; } = new List<string>();
    public List<PriceVariant> PriceVariants { get; set; } = new List<PriceVariant>();
    protected Product() { }
    public Product(int id,string title) {
        Id= id;
        Title= title;
    }
    public Product(ProductDto dto):this(dto.Id,dto.Title)
    {
        ImageUrl=dto.ImageUrl;
        Description=dto.Description;
        Price=dto.Price;
        IsAvailable=dto.IsAvailable;
        Categories=dto.Categories;
    }
}