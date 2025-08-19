using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using VenomPizzaMenuService.src.dto;

namespace VenomPizzaMenuService.src.model;

public class Product
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool Available { get; set; }
    public List<string>? Categories { get; set; }
    public Product(ProductDto dto)
    {
        Id=dto.Id;
        Title=dto.Title;
        ImageUrl=dto.ImageUrl;
        Description=dto.Description;
        Price=dto.Price;
        Available=dto.Available;
        Categories=dto.Categories;
    }
}