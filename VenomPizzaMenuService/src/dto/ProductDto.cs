using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.dto;
[JsonDerivedType(typeof(ProductDto),"Product")]
[JsonDerivedType(typeof(DishDto),"Dish")]
[JsonDerivedType(typeof(ComboDto), "Combo")]
public class ProductDto
{
    [Required]
    [Range(0, int.MaxValue,ErrorMessage ="Id не может быть отрицательным")]
    public int Id { get; set; }
    [Required]
    [NotNull]
    [Length(4,50,ErrorMessage ="Название продукта должно быть от 4 до 50 символов")]
    public string Title { get; set; }
    public string? ImageUrl { get; set; }
    [Length(0,500, ErrorMessage = "Описание продукта не может быть больше 500 символов")]
    public string? Description { get; set; }
    [Range(0,int.MaxValue,ErrorMessage ="Цена не может быть меньше 1 рубля")]
    public bool IsAvailable { get; set; } = true;
    public string? Unit { get; set; }
    public List<string> Categories { get; set; } = new List<string>();
    public string PriceVariants { get; set; }
    [NotMapped]
    public Dictionary<string, decimal> PriceVariantsDict
    {
        get => JsonSerializer.Deserialize<Dictionary<string, decimal>>(PriceVariants ?? "{}") ?? [];
        set => PriceVariants = JsonSerializer.Serialize(value);
    }
    public ProductDto(int id,string title)
    {
        Id = id;
        Title = title;
    }
    public virtual Product ToProduct()
    {
        return new Product(this);
    }
    public virtual void Validate()
    {
        var results=new List<ValidationResult>();
        var context=new ValidationContext(this);
        foreach (var t in PriceVariantsDict.Values)
            if (t < 0)
                results.Add(new ValidationResult("Цена должна быть положительной"));
        if (!Validator.TryValidateObject(this, context, results,true))
        {
            var errors=string.Join(", ",results.Select(x => x.ErrorMessage));
            throw new ValidationException(errors);
        }
    }
}