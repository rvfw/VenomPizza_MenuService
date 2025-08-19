using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace VenomPizzaMenuService.src.dto;

public class DishDto:ProductDto
{
    public List<string>? Ingredients { get; set; }
    public float Proteins { get; set; }
    public float Fats { get; set; }
    public float Carbohydrates { get; set; }
    public float Calorific { get; set; }
    public List<string>? Allergens { get; set; }
    public string? PriceVariants { get; set; }
    public string Unit { get; set; } = "";
    [NotMapped]
    public Dictionary<int, decimal> PriceVariantsDict
    {
        get => JsonSerializer.Deserialize<Dictionary<int, decimal>>(PriceVariants ?? "{}") ?? [];
        set => PriceVariants = JsonSerializer.Serialize(value);
    }
}
