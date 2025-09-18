using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.dto;

public class PriceVariantDto
{
    public int PriceId { get; set; }
    public string Size { get; set; } = "Стандартный";
    public decimal Price { get; set; }
    public PriceVariantDto(PriceVariant priceVariant)
    {
        PriceId = priceVariant.PriceId;
        Size=priceVariant.Size;
        Price=priceVariant.Price;
    }
}
