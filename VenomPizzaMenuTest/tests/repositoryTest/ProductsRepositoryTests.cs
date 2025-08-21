using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Text.Json;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.dto;
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

    #region create
    [Test]
    public async Task AddProduct_Success()
    {
        ProductDto[] products = { new ProductDto(1,"Product"), new DishDto(2,"Dish") { Calorific=100} };
        foreach (var product in products)
            await _productsRepository.AddProduct(product);
        var createdProducts=await _context.Products.OrderBy(p => p.Id).ToListAsync();
        Assert.That(createdProducts.Count, Is.EqualTo(2));
        for (int i = 0; i < products.Length; i++)
        {
            Assert.That(createdProducts[i].Id, Is.EqualTo(products[i].Id));
            Assert.That(createdProducts[i].Title, Is.EqualTo(products[i].Title));
        }
        Assert.That(((Dish)createdProducts[1]).Calorific, Is.EqualTo(100));

        var comboProducts=new Dictionary<int, int>() { [1] = 1, [2]= 2};
        var combo = new ComboDto(3, "Combo") { ProductsDict = comboProducts };
        await _productsRepository.AddProduct(combo);

        var foundedCombo = await _context.Products.FirstOrDefaultAsync(x=>x.Id==3);
        Assert.That(foundedCombo, Is.Not.Null);
        Assert.That(foundedCombo.Id, Is.EqualTo(combo.Id));
        Assert.That(foundedCombo.Title, Is.EqualTo(combo.Title));

        var foundedComboProducts= ((Combo)foundedCombo).Products;
        Assert.That(foundedComboProducts[0].ProductId, Is.EqualTo(products[0].Id));
        Assert.That(foundedComboProducts[1].ProductId, Is.EqualTo(products[1].Id));
        Assert.That(foundedComboProducts[0].Quantity, Is.EqualTo(1));
        Assert.That(foundedComboProducts[1].Quantity, Is.EqualTo(2));
    }
    [Test]
    public async Task AddProduct_AlreadyExists()
    {
        ProductDto product = new(1, "Product");
        _context.Products.Add(product.ToProduct());
        await _context.SaveChangesAsync();
        Assert.ThrowsAsync<ArgumentException>(async()=>await _productsRepository.AddProduct(product), "Товар с ID 1 уже существует");
    }
    [Test]
    public void AddCombo_WrongProductId()
    {
        var product = new ComboDto(1, "Combo") {ProductsDict=new() { [1]= 1 } };
        Assert.ThrowsAsync<ArgumentException>(async () => await _productsRepository.AddProduct(product), "Товара с ID 1 не существует");
    }
    #endregion

    #region read
    [Test]
    public async Task GetProductById_Success()
    {
        var product = new Dish(1, "Dish") { Calorific = 100 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result=await _productsRepository.GetProductById(product.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Title, Is.EqualTo("Dish"));
        Assert.That(((Dish)result).Calorific, Is.EqualTo(100));
    }
    [Test]
    public void GetProductById_IdNotFound()
    {
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>await _productsRepository.GetProductById(1));
    }
    [Test]
    public async Task GetProductsPage_FirstPage()
    {
        var products = new Product[] { new Product(1, "Product"), new Dish(2, "Dish") { Calorific=100}, new Combo(3, "Combo") };
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
        Assert.That(((Dish)result[1]).Calorific, Is.EqualTo(100));
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
    #endregion

    #region update
    [Test]
    public async Task UpdateProductInfo_Success()
    {
        _context.Products.Add(new Dish(1, "Dish") { Calorific = 100 });
        await _context.SaveChangesAsync();
        var newInfo = new DishDto(1, "NewDish") { Calorific = 200 };
        await _productsRepository.UpdateProductInfo(newInfo);
        var updatedProduct=_context.Products.FirstOrDefault(x=>x.Id==1);
        Assert.That(updatedProduct, Is.Not.Null);
        Assert.That(updatedProduct.Title, Is.EqualTo(newInfo.Title));
        Assert.That(((Dish)updatedProduct).Calorific, Is.EqualTo(200));
    }
    [Test]
    public void UpdateProductInfo_NotFound()
    {
        var newInfo = new DishDto(1, "NewDish") { Calorific = 200 };
        Assert.ThrowsAsync<KeyNotFoundException>(async()=> await _productsRepository.UpdateProductInfo(newInfo), "Товара с ID 1 не найдено");
    }
    #endregion

    #region delete
    [Test]
    public async Task DeleteProductById_Success()
    {
        var product = new Dish(1, "Dish") { Calorific = 100 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        Assert.That(_context.Products.Count, Is.EqualTo(1));
        await _productsRepository.DeleteProductById(product.Id);
        Assert.That(_context.Products.Count,Is.EqualTo(0));
    }
    #endregion
}
