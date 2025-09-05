using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using VenomPizzaMenuService.src.dto;

namespace VenomPizzaMenuService.src.model;

[Table("dishes")]
public class Dish : Product
{
    [Column("ingridients")]
    public List<string> Ingredients { get; set; } = new List<string>();
    [Column("proteins")]
    public float Proteins { get; set; }
    [Column("fats")]
    public float Fats { get; set; }
    [Column("carbohydrates")]
    public float Carbohydrates { get; set; }
    [Column("calorific")]
    public float Calorific { get; set; }
    [Column("allergens")]
    public List<string> Allergens { get; set; } = new List<string>();
    [Column("unit")]
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
