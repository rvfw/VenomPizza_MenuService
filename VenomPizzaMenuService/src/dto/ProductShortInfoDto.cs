using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.dto;

public class ProductShortInfoDto
{
    public int Id {get; set;}
    public string Title {get; set;}
    public string? ImageUrl { get; set;}
    public bool IsAvailable { get; set;}
    public decimal Price { get; set;}
    public ProductShortInfoDto(Product p)
    {
        Id=p.Id; 
        Title=p.Title;
        ImageUrl=p.ImageUrl;
        IsAvailable=p.IsAvailable; 
        Price = p.Price;
    }
    public ProductShortInfoDto(int id, string title)
    {
        Id=id; 
        Title=title;
    }
}
