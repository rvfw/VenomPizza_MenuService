using System.ComponentModel.DataAnnotations;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuTest.tests.dtoTest;

public class ValidateDtoTests
{
    private readonly string[] validTitles = { "ABCD" };
    private readonly string?[] validDescriptions = { null,"","A" };

    private readonly string?[] wrongTitles = {null, "","ABC",new string('W',51) };
    private readonly string?[] wrongDescriptions = { new string('W', 501) };

    [Test]
    public void ProductDto_Success()
    {
        foreach (var title in validTitles)
        {
            var dto = new ProductDto(1, title) { Description = validDescriptions[0] };
            Assert.DoesNotThrow(dto.Validate);
        }
        foreach (var description in validDescriptions)
        {
            var dto = new ProductDto(1, validTitles[0]) { Description = description };
            Assert.DoesNotThrow(dto.Validate);
        }
    }
    [Test]
    public void ProductDto_WrongTitle()
    {
        foreach (var title in wrongTitles)
        {
            var dto = new ProductDto(1, title) { Description = validDescriptions[0] };
            Assert.Throws<ValidationException>(dto.Validate);
        }
    }
    [Test]
    public void ProductDto_WrongDescription()
    {
        foreach (var description in wrongDescriptions)
        {
            var dto = new ProductDto(1, validTitles[0]) { Description = description };
            Assert.Throws<ValidationException>(dto.Validate);
        }
    }
    [Test]
    public void ProductDto_WrongId()
    {
        var dto = new ProductDto(-1, validTitles[0]);
        Assert.Throws<ValidationException>(dto.Validate);
    }
    [Test]
    public void DishDto_WrongNutritionalValues()
    {
        DishDto[] dtos= { 
            new DishDto(1, validTitles[0]) { Proteins = -1 },
            new DishDto(2, validTitles[0]) { Fats = -1 },
            new DishDto(3, validTitles[0]) { Calorific = -1 },
            new DishDto(4, validTitles[0]) { Carbohydrates = -1 },
        };
        foreach (var dto in dtos)
            Assert.Throws<ValidationException>(dto.Validate);
    }
}
