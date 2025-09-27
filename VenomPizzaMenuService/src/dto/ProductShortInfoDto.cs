using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.dto;

public class ProductShortInfoDto
{
    public int Id {get; set;}
    public string Title {get; set;}
    public string? ImageUrl { get; set;}
    public bool IsAvailable { get; set;}
    public string? Unit { get; set;}
    public List<PriceVariantDto> Prices { get; set;}
    public ProductShortInfoDto() { }
    public ProductShortInfoDto(Product p)
    {
        Id=p.Id; 
        Title=p.Title;
        ImageUrl=p.ImageUrl;
        IsAvailable=p.IsAvailable;
        Unit = p.Unit;
        Prices = p.PriceVariants.Select(x=>new PriceVariantDto(x)).ToList();
    }
    public ProductShortInfoDto(int id, string title)
    {
        Id=id; 
        Title=title;
    }
}