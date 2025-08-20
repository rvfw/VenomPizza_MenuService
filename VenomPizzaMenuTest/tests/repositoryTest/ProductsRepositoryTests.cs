using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuService.tests.repositoryTest;

[TestFixture]
public class ProductsRepositoryTests
{
    private DbContextOptions<ProductsDbContext> _contextOptions;
    private ProductsDbContext _context;
    private ProductsRepository _productsRepository;

    [SetUp]
    public void Setup()
    {
        _contextOptions = new DbContextOptionsBuilder<ProductsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context=new ProductsDbContext(_contextOptions);
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
        var product = new Product(1, "Product");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var result=await _productsRepository.GetProductById(product.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Title, Is.EqualTo("Product"));
    }
    [Test]
    public void GetProductById_IdNotFound()
    {
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>await _productsRepository.GetProductById(1));
    }
    [Test]
    public void GetProductById_WrongId()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await _productsRepository.GetProductById(0));
    }
    [Test]
    public async Task GetProductsPage_FirstPage()
    {
        var products = new Product[] { new Product(1, "Product"), new Dish(2, "Dish"), new Combo(3, "Combo") };
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        var result = await _productsRepository.GetProductsPage(1,2);

        Assert.That(result.Count, Is.EqualTo(2));
        for (int i=0;i<result.Count();i++)
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

        var result = await _productsRepository.GetProductsPage(2, 2);

        Assert.That(result.Count,Is.EqualTo(1));
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

        var result = await _productsRepository.GetProductsPage(1, 25);

        Assert.That(result.Count, Is.EqualTo(3));
        for (int i = 0; i < result.Count(); i++)
        {
            Assert.That(result[i], Is.Not.Null);
            Assert.That(result[i].Id, Is.EqualTo(products[i].Id));
            Assert.That(result[i].Title, Is.EqualTo(products[i].Title));
        }
    }
    [Test]
    public void GetProductsPage_WrongPage()
    {
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _productsRepository.GetProductsPage(1, 1), "Страницы 1 не существует");
    }
    [Test]
    public void GetProductsPage_BadPageRequest()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await _productsRepository.GetProductsPage(0, 1), "Номер страницы и ее размер должны быть больше 0");
    }
    [Test]
    public void GetProductsPage_BadSizeRequest()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await _productsRepository.GetProductsPage(1, 0), "Номер страницы и ее размер должны быть больше 0");
    }
}
