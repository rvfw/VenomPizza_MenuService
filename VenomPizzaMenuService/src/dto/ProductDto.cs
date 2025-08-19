using System.ComponentModel.DataAnnotations;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.dto;

public class ProductDto
{
    public required string Title { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    [Range(1,99999)]
    public decimal Price { get; set; }
    public bool Available { get; set; } = true;
    public List<string>? Categories { get; set; }
    public ProductType ProductType { get; set; }
}
public enum ProductType
{
    Product,
    Combo,
    Dish,
}