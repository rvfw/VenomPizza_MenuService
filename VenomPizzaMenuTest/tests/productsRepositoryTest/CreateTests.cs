using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuTest.tests.productsRepositoryTest;

internal class CreateTests
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
    public async Task AddProduct_Success()
    {
        ProductDto product = new ProductDto(1, "Product");
        await _productsRepository.AddProduct(product);
        var createdProduct = _context.Products.First();

        Assert.That(createdProduct.Id, Is.EqualTo(product.Id));
        Assert.That(createdProduct.Title, Is.EqualTo(product.Title));
    }

    [Test]
    public async Task AddDish_Success()
    {
        DishDto dish = new DishDto(1, "Dish") { Calorific = 100, Ingredients = new() { "cheese" }, Allergens = new() { "peanuts" } };
        await _productsRepository.AddProduct(dish);
        var createdProduct = (Dish)_context.Products.First();

        Assert.That(createdProduct.Id, Is.EqualTo(dish.Id));
        Assert.That(createdProduct.Title, Is.EqualTo(dish.Title));
        Assert.That(createdProduct.Calorific, Is.EqualTo(dish.Calorific));
        Assert.That(createdProduct.Ingredients[0], Is.EqualTo(dish.Ingredients[0]));
        Assert.That(createdProduct.Allergens[0], Is.EqualTo(dish.Allergens[0]));
    }

    [Test]
    public async Task AddCombo_Success()
    {
        var product = new Product(1, "Product");
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        ComboDto combo = new ComboDto(2, "Combo") { ProductsDict = new() { [1] = 3 } };
        await _productsRepository.AddProduct(combo);
        var createdProduct = (Combo)_context.Products.Last();

        Assert.That(createdProduct.Id, Is.EqualTo(combo.Id));
        Assert.That(createdProduct.Title, Is.EqualTo(combo.Title));
        Assert.That(createdProduct.Products.Count(), Is.EqualTo(1));
        Assert.That(createdProduct.Products[0].Quantity, Is.EqualTo(combo.ProductsDict[1]));
        Assert.That(createdProduct.Products[0].Product.Id, Is.EqualTo(product.Id));
        Assert.That(createdProduct.Products[0].Product.Title, Is.EqualTo(product.Title));
    }

    [Test]
    public async Task AddProduct_AlreadyExists()
    {
        ProductDto product = new(1, "Product");
        _context.Products.Add(product.ToProduct());
        await _context.SaveChangesAsync();

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _productsRepository.AddProduct(product), "Товар с ID 1 уже существует");
    }

    [Test]
    public async Task AddProduct_AddPrices_Success()
    {
        ProductDto product = new ProductDto(1, "Product") { PriceVariantsDict = new() { ["10"] = 100 } };
        await _productsRepository.AddProduct(product);
        var createdProduct = _context.Products.First();

        Assert.That(createdProduct.PriceVariants.Count, Is.EqualTo(1));
        Assert.That(createdProduct.PriceVariants.FirstOrDefault(x => x.ProductId == 1), Is.Not.Null);
        Assert.That(createdProduct.PriceVariants.FirstOrDefault(x => x.Size == "10"), Is.Not.Null);
        Assert.That(createdProduct.PriceVariants[0].Price, Is.EqualTo(product.PriceVariantsDict["10"]));
    }
}
