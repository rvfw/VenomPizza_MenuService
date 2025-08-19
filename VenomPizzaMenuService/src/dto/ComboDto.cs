using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.dto;

public class ComboDto:ProductDto
{
    public string Products { get; set; }
    [NotMapped]
    public Dictionary<int, int> ProductsDict {
        get => JsonSerializer.Deserialize<Dictionary<int, int>>(Products ?? "{}") ?? [];
    }
}
