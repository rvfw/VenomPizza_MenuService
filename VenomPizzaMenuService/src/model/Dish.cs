using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using VenomPizzaMenuService.src.dto;

namespace VenomPizzaMenuService.src.model;

public class Dish : Product
{
    public List<string>? Ingredients { get; set; }
    public float Proteins { get; set; }
    public float Fats { get; set; }
    public float Carbohydrates { get; set; }
    public float Calorific { get; set; }
    public List<string>? Allergens { get; set; }
    public string? PriceVariants { get; set; }
    public string? Unit { get; set; }
    [NotMapped]
    public Dictionary<int, decimal> PriceVariantsDict
    {
        get => JsonSerializer.Deserialize<Dictionary<int, decimal>>(PriceVariants ?? "{}") ?? [];
        set => PriceVariants = JsonSerializer.Serialize(value);
    }
    public Dish(DishDto dto):base(dto)
    {
        Ingredients = dto.Ingredients;
        Proteins = dto.Proteins;
        Fats = dto.Fats;
        Carbohydrates = dto.Carbohydrates;
        Calorific = dto.Calorific;
        Allergens = dto.Allergens;
        PriceVariants = dto.PriceVariants;
        Unit = dto.Unit;
    }
}
