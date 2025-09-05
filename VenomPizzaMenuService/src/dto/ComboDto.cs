using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.dto;

public class ComboDto:ProductDto
{
    [NotMapped]
    public string Products { get; set; } = "{}";
    [NotMapped]
    public Dictionary<int, int> ProductsDict {
        get => JsonSerializer.Deserialize<Dictionary<int, int>>(Products ?? "{}") ?? [];
        set => Products = JsonSerializer.Serialize(value);
    }
    public ComboDto(int id, string? title) : base(id, title) { }
    public override Product ToProduct()
    {
        return new Combo(this);
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
