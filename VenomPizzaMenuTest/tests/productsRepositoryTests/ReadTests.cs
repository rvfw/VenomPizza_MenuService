using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuTest.tests.productsRepositoryTest;

[TestFixture]
internal class ReadTests
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
    public async Task GetProductById_Success()
    {
        var product = new Dish(1, "Product");
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _productsRepository.GetProductById(product.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(product.Id));
        Assert.That(result.Title, Is.EqualTo(product.Title));
    }

    [Test]
    public async Task GetDishById_Success()
    {
        var dish = new Dish(1, "Dish") { Calorific = 100, Ingredients = new() { "cheese" }, Allergens = new() { "peanuts" } };
        _context.Products.Add(dish);
        await _context.SaveChangesAsync();

        var result = (Dish)await _productsRepository.GetProductById(dish.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(dish.Id));
        Assert.That(result.Title, Is.EqualTo(dish.Title));
        Assert.That(result.Calorific, Is.EqualTo(dish.Calorific));
        Assert.That(result.Ingredients[0], Is.EqualTo(dish.Ingredients[0]));
        Assert.That(result.Allergens[0], Is.EqualTo(dish.Allergens[0]));
    }

    [Test]
    public async Task GetComboById_Success()
    {
        var product = new Product(1, "Product");
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        Combo combo = new Combo(2, "Combo") { Products = new() { new ComboProduct(2, 1, 3) } };
        _context.Products.Add(combo);
        await _context.SaveChangesAsync();

        var foundedProduct = await _productsRepository.GetProductById(combo.Id);
        if (foundedProduct == null || !(foundedProduct is Combo))
            Assert.Fail("Неверный тип продукта или продукт не найден");
        var result = (Combo)foundedProduct!;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(combo.Id));
        Assert.That(result.Title, Is.EqualTo(combo.Title));
        Assert.That(result.Products[0].ProductId, Is.EqualTo(combo.Products[0].ProductId));
        Assert.That(result.Products[0].ComboId, Is.EqualTo(combo.Id));
        Assert.That(result.Products[0].Quantity, Is.EqualTo(combo.Products[0].Quantity));
    }

    [Test]
    public async Task GetProductsPage_FirstPage()
    {
        var products = new Product[] { new Product(1, "Product"), new Dish(2, "Dish") { Calorific = 100 }, new Combo(3, "Combo") };
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        var result = await _productsRepository.GetProductsPage(0, 2);

        Assert.That(result.Count, Is.EqualTo(2));
        for (int i = 0; i < result.Count(); i++)
        {
            Assert.That(result[i], Is.Not.Null);
            Assert.That(result[i].Id, Is.EqualTo(products[i].Id));
            Assert.That(result[i].Title, Is.EqualTo(products[i].Title));
        }
    }

    [Test]
    public async Task GetProductsPage_SecondPage()
    {
        var products = new Product[] { new Product(1, "Product"), new Dish(2, "Dish"), new Combo(3, "Combo") };
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        var result = await _productsRepository.GetProductsPage(1, 2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0], Is.Not.Null);
        Assert.That(result[0].Id, Is.EqualTo(products[2].Id));
        Assert.That(result[0].Title, Is.EqualTo(products[2].Title));
    }

    [Test]
    public async Task GetProductsPage_AllProducts()
    {
        var products = new Product[] { new Product(1, "Product"), new Dish(2, "Dish"), new Combo(3, "Combo") };
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        var result = await _productsRepository.GetProductsPage(0, 3);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));
        for (int i = 0; i < result.Count(); i++)
        {
            Assert.That(result[i], Is.Not.Null);
            Assert.That(result[i].Id, Is.EqualTo(products[i].Id));
            Assert.That(result[i].Title, Is.EqualTo(products[i].Title));
        }
    }

    [Test]
    public async Task GetProductsByCategory_Success()
    {
        var products = new Product[] { new Product(1, "Product") { Categories = new() { "Product" } }, new Dish(2, "Dish") { Categories = new() { "Product", "Dish" } } };
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        var productCategory = await _productsRepository.GetProductsByCategory("Product");
        var dishCategory = await _productsRepository.GetProductsByCategory("Dish");

        Assert.That(productCategory, Is.Not.Null);
        Assert.That(productCategory.Count, Is.EqualTo(2));
        for (int i = 0; i < productCategory.Count(); i++)
        {
            Assert.That(productCategory[i], Is.Not.Null);
            Assert.That(productCategory[i].Id, Is.EqualTo(products[i].Id));
            Assert.That(productCategory[i].Title, Is.EqualTo(products[i].Title));
        }
        Assert.That(dishCategory, Is.Not.Null);
        Assert.That(dishCategory.Count, Is.EqualTo(1));
        Assert.That(dishCategory[0], Is.Not.Null);
        Assert.That(dishCategory[0].Id, Is.EqualTo(products[1].Id));
        Assert.That(dishCategory[0].Title, Is.EqualTo(products[1].Title));
    }

    [Test]
    public async Task GetProductIdAndTitle_Success()
    {
        var product = new Product(1, "product") { PriceVariants = new() { new PriceVariant() { PriceId = 0, ProductId = 1, Size="XXL" } } };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var idAndTitle = await _productsRepository.GetProductIdAndTitle(product.Id, 0);

        Assert.That(idAndTitle, Is.Not.Null);
        Assert.That(idAndTitle.Value.id, Is.EqualTo(product.Id));
        Assert.That(idAndTitle.Value.title, Is.EqualTo(product.Title));
    }

    [Test]
    public async Task GetProductIdAndTitle_PriceNotFound()
    {
        var product = new Product(1, "product") { PriceVariants = new() { new PriceVariant() { PriceId = 0, ProductId = 1, Size = "XXL" } } };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var idAndTitle = await _productsRepository.GetProductIdAndTitle(product.Id, 1);

        Assert.That(idAndTitle, Is.Null);
    }

    [Test]
    public async Task GetProductIdAndTitle_ProductNotFound()
    {
        var idAndTitle = await _productsRepository.GetProductIdAndTitle(1, 0);

        Assert.That(idAndTitle, Is.Null);
    }

    [Test]
    public async Task CheckComboProducts_Success()
    {
        var product = new Product(1, "Product");
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        var combo = new ComboDto(1, "Combo") { ProductsDict = new() { [1] = 1 } };

        var allComboProductsExist = await _productsRepository.AllComboProductsExist(combo);
        var comboInCombo=await _productsRepository.ComboContainsCombo(combo);

        Assert.That(allComboProductsExist, Is.True);
        Assert.That(comboInCombo, Is.False);
    }

    [Test]
    public async Task CheckComboProducts_ProductNotFound()
    {
        var combo = new ComboDto(1, "Combo") { ProductsDict = new() { [1] = 1 } };

        var allComboProductsExist= await _productsRepository.AllComboProductsExist(combo);

        Assert.That(allComboProductsExist, Is.False);
    }

    [Test]
    public async Task CheckComboProducts_ComboInCombo()
    {
        var anotherCombo = new Combo(1, "Combo") { Products = new() };
        _context.Products.Add(anotherCombo);
        await _context.SaveChangesAsync();
        var combo = new ComboDto(2, "Combo") { ProductsDict = new() { [1] = 1 } };

        var comboInCombo = await _productsRepository.ComboContainsCombo(combo);

        Assert.That(comboInCombo, Is.True);
    }
}
