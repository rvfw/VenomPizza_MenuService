using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using VenomPizzaMenuService.src.dto;

namespace VenomPizzaMenuService.src.model;

public class Product
{
    public int Id { get; init; }
    public string Title { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool Available { get; set; }
    public List<string>? Categories { get; set; }
    public Product(int id,string title) {
        Id= id;
        Title= title;
    }
    public Product(ProductDto dto):this(dto.Id,dto.Title)
    {
        ImageUrl=dto.ImageUrl;
        Description=dto.Description;
        Price=dto.Price;
        Available=dto.Available;
        Categories=dto.Categories;
    }
}