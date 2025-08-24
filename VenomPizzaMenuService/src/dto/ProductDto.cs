using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.dto;
[JsonDerivedType(typeof(ProductDto),"Product")]
[JsonDerivedType(typeof(DishDto),"Dish")]
[JsonDerivedType(typeof(ComboDto), "Combo")]
public class ProductDto
{
    [Required]
    [Range(1, int.MaxValue,ErrorMessage ="ID не может быть меньше 1")]
    public int Id { get; set; }
    [Required]
    [NotNull]
    [Length(4,50,ErrorMessage ="Название продукта должно быть от 4 до 50 символов")]
    public string? Title { get; set; }
    public string? ImageUrl { get; set; }
    [Length(0,500, ErrorMessage = "Описание продукта не может быть больше 500 символов")]
    public string? Description { get; set; }
    [Range(0,int.MaxValue,ErrorMessage ="Цена не может быть меньше 1 рубля")]
    public decimal Price { get; set; }
    public bool Available { get; set; } = true;
    public List<string> Categories { get; set; } = new List<string>();
    public ProductDto(int id,string? title)
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
        if (!Validator.TryValidateObject(this, context, results,true))
        {
            var errors=string.Join(", ",results.Select(x => x.ErrorMessage));
            throw new ValidationException(errors);
        }
    }
}