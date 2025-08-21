using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.dto;

public class DishDto:ProductDto
{
    public List<string>? Ingredients { get; set; }
    [Range(0, int.MaxValue,ErrorMessage ="Белки не могут быть отрицательного значения")]
    public float Proteins { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Жиры не могут быть отрицательного значения")]
    public float Fats { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Углеводы не могут быть отрицательного значения")]
    public float Carbohydrates { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Каллории не могут быть отрицательного значения")]
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
    public DishDto(int id,string? title) : base(id,title) { }
    public override Product ToProduct()
    {
        return new Dish(this);
    }
    public override void Validate()
    {
        base.Validate();
        var results = new List<ValidationResult>();
        var context = new ValidationContext(this);
        if (!Validator.TryValidateObject(this, context, results, true))
        {
            var errors = string.Join(", ", results.Select(x => x.ErrorMessage));
            throw new ValidationException(errors);
        }
    }
}
