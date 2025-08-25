using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace VenomPizzaMenuService.src.model;

public class PriceVariant
{
    [Key]
    public int Id { get; set; }
    public int ProductId { get; set; }
    [JsonIgnore]
    public Product Product { get; set; }
    public string Size { get; set; }
    public decimal Price { get; set; }
    public PriceVariant() { }
    public PriceVariant(Product product, string size,decimal price)
    {
        Product=product;
        Size = size;
        Price = price;
    }
}
