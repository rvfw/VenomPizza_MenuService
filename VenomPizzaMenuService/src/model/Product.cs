
using VenomPizzaMenuService.src.dto;

namespace VenomPizzaMenuService.src.model;

public class Product
{
    public int Id { get; init; }
    public string Title { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> Categories { get; set; } = new List<string>();
    public List<PriceVariant> PriceVariants { get; set; } = new List<PriceVariant>();
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