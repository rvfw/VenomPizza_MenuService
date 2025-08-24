using System.ComponentModel.DataAnnotations;

namespace VenomPizzaMenuService.src.model;

public class PriceVariant
{
    [Key]
    public int Id { get; set; }
    public int DishId { get; set; }
    public Dish Dish { get; set; }
    public string Size { get; set; }
    public decimal Price { get; set; }
    public PriceVariant() { }
    public PriceVariant(Dish dish,decimal price, string size)
    {
        this.Dish=dish;
        Size = size;
        Price = price;
    }
}
