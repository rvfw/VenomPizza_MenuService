using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.dto;

public class ProductInMenuDto
{
    public int Id {get; set;}
    public string Title {get; set;}
    public string Description { get; set; }
    public bool IsAvailable { get; set;}
    public decimal Price { get; set;}
    public ProductInMenuDto(Product p)
    {
        Id=p.Id; 
        Title=p.Title;
        Description = p.Description;
        IsAvailable=p.IsAvailable; 
        Price = p.Price;
    }
}
