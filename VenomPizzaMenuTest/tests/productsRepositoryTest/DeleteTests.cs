using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuTest.tests.productsRepositoryTest;

[TestFixture]
internal class DeleteTests
{
    private DbContextOptions<ProductsDbContext> _contextOptions;
    private ProductsDbContext _context;
    private IProductsRepository _productsRepository;

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
    public async Task DeleteProductById_Success()
    {
        var product = new Dish(1, "Product");
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var res=await _productsRepository.DeleteProductById(product.Id);

        Assert.That(_context.Products.Count, Is.EqualTo(0));
        Assert.That(res, Is.True);
    }

    [Test]
    public async Task DeleteComboById_Success()
    {
        var dish = new Dish(1, "Dish");
        _context.Products.Add(dish);
        await _context.SaveChangesAsync();
        var combo = new Combo(2, "Combo") { Products = new() { new ComboProduct(2, 1, 3) } };
        _context.Products.Add(combo);
        await _context.SaveChangesAsync();

        await _productsRepository.DeleteProductById(combo.Id);

        Assert.That(_context.Products.Count, Is.EqualTo(1));
        Assert.That(_context.Combos.Count, Is.EqualTo(0));
        Assert.That(_context.ComboProducts.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteProductById_NotFound()
    {
        var res=await _productsRepository.DeleteProductById(1);

        Assert.That(res, Is.False);
    }
}
