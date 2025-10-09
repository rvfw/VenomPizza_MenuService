using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuTest.tests.productsRepositoryTest;

[TestFixture]
internal class UpdateTests
{
    private DbContextOptions<ProductsDbContext> _contextOptions;
    private ProductsDbContext _context;
    private ProductsRepository _productsRepository;

    [SetUp]
    public void Setup()
    {
        _contextOptions = new DbContextOptionsBuilder<ProductsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new ProductsDbContext(_contextOptions);
        _productsRepository = new ProductsRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureCreated();
        _context.Dispose();
    }

    [Test]
    public async Task UpdateProductInfo_Success()
    {
        _context.Products.Add(new Product(1, "Product"));
        await _context.SaveChangesAsync();

        var newInfo = new ProductDto(1, "NewProduct");
        await _productsRepository.UpdateProductInfo(newInfo);
        var updatedProduct = _context.Products.FirstOrDefault(x => x.Id == 1);

        Assert.That(updatedProduct, Is.Not.Null);
        Assert.That(updatedProduct.Title, Is.EqualTo(newInfo.Title));
    }

    [Test]
    public async Task UpdateProductPrices_Success()
    {
        var price1 = new PriceVariant() { PriceId = 0, ProductId = 1, Size = "L", Price = 50 };
        var price2 = new PriceVariant() { PriceId = 1, ProductId = 1, Size = "XL", Price = 100 };
        _context.Products.Add(new Product(1, "Product") { PriceVariants = new() {  } });
        await _context.SaveChangesAsync();

        var newInfo = new ProductDto(1, "Product") { PriceVariantsDict = new() { ["XL"] = 200, ["XXL"]=300} };
        await _productsRepository.UpdateProductInfo(newInfo);
        var updatedProduct = _context.Products.FirstOrDefault(x => x.Id == 1);
        var newPrice1 = _context.PriceVariants.First(x => x.PriceId == 0);
        var newPrice2 = _context.PriceVariants.First(x => x.PriceId == 1);

        Assert.That(updatedProduct, Is.Not.Null);
        Assert.That(_context.PriceVariants.Count(),Is.EqualTo(2));
        Assert.That(newPrice1.Size, Is.EqualTo("XL"));
        Assert.That(newPrice1.Price, Is.EqualTo(200));
        Assert.That(newPrice2.Size, Is.EqualTo("XXL"));
        Assert.That(newPrice2.Price, Is.EqualTo(300));
    }

    [Test]
    public async Task UpdateDishInfo_Success()
    {
        _context.Products.Add(new Dish(1, "Dish") { Calorific = 100 });
        await _context.SaveChangesAsync();

        var newInfo = new DishDto(1, "Dish") { Calorific = 200 };
        await _productsRepository.UpdateProductInfo(newInfo);
        if (!(_context.Products.FirstOrDefault(x => x.Id == 1) is Dish updatedDish))
        {
            Assert.Fail("Продукт неправильного типа");
            return;
        }

        Assert.That(updatedDish.Calorific, Is.EqualTo(newInfo.Calorific));
    }

    [Test]
    public async Task UpdateComboInfo_Success()
    {
        _context.Products.Add(new Product(1, "Product"));
        await _context.SaveChangesAsync();
        _context.Products.Add(new Dish(2, "Dish"));
        await _context.SaveChangesAsync();
        _context.Products.Add(new Combo(3, "Combo") { Products = new() { new ComboProduct(3, 1, 1) } });
        await _context.SaveChangesAsync();

        var newInfo = new ComboDto(3, "Combo") { ProductsDict = new() { [2]=1 } };
        await _productsRepository.UpdateProductInfo(newInfo);
        if (!(_context.Products.FirstOrDefault(x => x.Id == 3) is Combo updatedCombo))
        {
            Assert.Fail("Продукт неправильного типа");
            return;
        }
        var newComboProduct = _context.ComboProducts.First();

        Assert.That(updatedCombo.Products.Count, Is.EqualTo(1));
        Assert.That(_context.ComboProducts.Count, Is.EqualTo(1));
        Assert.That(newComboProduct.ProductId, Is.EqualTo(2));
        Assert.That(newComboProduct.Quantity, Is.EqualTo(1));
    }

    [Test]
    public void UpdateProductInfo_NotFound()
    {
        var newInfo = new DishDto(1, "NewDish") { Calorific = 200 };
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _productsRepository.UpdateProductInfo(newInfo), "Товара с ID 1 не найдено");
    }
}
