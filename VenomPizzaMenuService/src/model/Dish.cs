using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using VenomPizzaMenuService.src.dto;

namespace VenomPizzaMenuService.src.model;

public class Dish : Product
{
    public List<string> Ingredients { get; set; } = new List<string>();
    public float Proteins { get; set; }
    public float Fats { get; set; }
    public float Carbohydrates { get; set; }
    public float Calorific { get; set; }
    public List<string> Allergens { get; set; } = new List<string>();
    public string? Unit { get; set; }

    public Dish(int id, string title) : base(id, title) { }
    public Dish(DishDto dto):base(dto)
    {
        Ingredients = dto.Ingredients;
        Proteins = dto.Proteins;
        Fats = dto.Fats;
        Carbohydrates = dto.Carbohydrates;
        Calorific = dto.Calorific;
        Allergens = dto.Allergens;
        Unit = dto.Unit;
    }
}
