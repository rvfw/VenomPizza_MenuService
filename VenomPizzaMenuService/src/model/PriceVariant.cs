using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace VenomPizzaMenuService.src.model;
[Table("price_variants")]
public class PriceVariant
{
    [Key]
    [Column("price_id")]
    public int PriceId { get; set; }
    [Column("product_id")]
    public int ProductId { get; set; }
    [JsonIgnore]
    public Product Product { get; set; }
    [Column("size")]
    public string Size { get; set; }
    [Column("price")]
    public decimal Price { get; set; }
    public PriceVariant() { }
    public PriceVariant(Product product,int id, string size,decimal price)
    {
        PriceId=id;
        Product=product;
        Size = size;
        Price = price;
    }
}
