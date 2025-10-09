using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using VenomPizzaMenuService.src.cache;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuTest.tests.productServiceTests;

[TestFixture]
internal class ReadTests
{
    private Mock<IProductsRepository> _mockProductsRepository;
    private Mock<ICacheProvider> _mockCacheProvider;
    private Mock<IProducer<string, string>> _mockProducer;
    private KafkaSettings _kafkaSettings;
    private Mock<ILogger<ProductsService>> _mockLogger;
    private IProductsService _productsService;

    [SetUp]
    public void Setup()
    {
        _mockProductsRepository = new Mock<IProductsRepository>();
        _mockCacheProvider = new Mock<ICacheProvider>();
        _mockProducer = new Mock<IProducer<string, string>>();
        _mockLogger = new Mock<ILogger<ProductsService>>();
        _kafkaSettings = new KafkaSettings
        {
            Topics = new KafkaTopics
            {
                ProductUpdated = "product-updated-topic",
            },
            BootstrapServers = "localhost:9092"
        };
        _productsService = new ProductsService(_mockProductsRepository.Object, _mockProducer.Object, Options.Create(_kafkaSettings), _mockLogger.Object, _mockCacheProvider.Object);
    }

    [Test]
    public async Task GetProductById_FoundedInCache()
    {
        int id = 1;
        _mockCacheProvider.Setup(provider => provider.GetAsync<Product>($"product:{id}"))
            .ReturnsAsync(new Product(1,"Product"));

        var foundedProduct=await _productsService.GetProductById(id);

        Assert.That(foundedProduct.Id, Is.EqualTo(id));
        _mockCacheProvider.Verify(provider => provider.GetAsync<Product>($"product:{id}"), Times.Once);
        _mockProductsRepository.Verify(repository => repository.GetProductById(It.IsAny<int>()), Times.Never);
        _mockCacheProvider.Verify(provider => provider.SetAsync<Product>(It.IsAny<string>(), It.IsAny<Product>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Test]
    public async Task GetProductById_FoundedInRepository()
    {
        int id = 1;
        _mockCacheProvider.Setup(provider => provider.GetAsync<Product>($"product:{id}"))
            .ReturnsAsync(()=>null);
        _mockProductsRepository.Setup(repository => repository.GetProductById(id))
            .ReturnsAsync(new Product(1, "Product"));

        var foundedProduct = await _productsService.GetProductById(id);

        Assert.That(foundedProduct.Id, Is.EqualTo(id));
        _mockCacheProvider.Verify(provider => provider.GetAsync<Product>($"product:{id}"), Times.Once);
        _mockProductsRepository.Verify(repository => repository.GetProductById(id), Times.Once);
        _mockCacheProvider.Verify(provider => provider.SetAsync<Product>($"product:{id}", foundedProduct, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Test]
    public void GetProductById_NotFound()
    {
        int id = 1;
        _mockCacheProvider.Setup(provider => provider.GetAsync<Product>($"product:{id}"))
            .ReturnsAsync(() => null);
        _mockProductsRepository.Setup(repository => repository.GetProductById(id))
            .ReturnsAsync(() => null);

        Assert.ThrowsAsync<KeyNotFoundException>(async()=> await _productsService.GetProductById(id));

        _mockCacheProvider.Verify(provider => provider.GetAsync<Product>($"product:{id}"), Times.Once);
        _mockProductsRepository.Verify(repository => repository.GetProductById(id), Times.Once);
        _mockCacheProvider.Verify(provider => provider.SetAsync<Product>(It.IsAny<string>(), It.IsAny<Product>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Test]
    public async Task GetProductsPage_FoundInCache()
    {
        int page=1, size=10;
        _mockCacheProvider.Setup(provider => provider.GetAsync<List<ProductShortInfoDto>>($"products:page:{page}:size:{size}"))
            .ReturnsAsync(()=>new() {new ProductShortInfoDto(1,"Product") });

        var foundedPage =await _productsService.GetProductsPage(page, size);

        Assert.That(foundedPage.Count, Is.EqualTo(1));
        Assert.That(foundedPage[0].Id, Is.EqualTo(1));
        _mockCacheProvider.Verify(provider => provider.GetAsync<List<ProductShortInfoDto>>($"products:page:{page}:size:{size}"), Times.Once);
        _mockProductsRepository.Verify(repository => repository.GetProductsPage(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _mockCacheProvider.Verify(provider => provider.SetAsync<Product>(It.IsAny<string>(), It.IsAny<Product>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Test]
    public async Task GetProductsPage_FoundInRepository()
    {
        int page = 1, size = 10;
        _mockCacheProvider.Setup(provider => provider.GetAsync<List<ProductShortInfoDto>>($"products:page:{page}:size:{size}"))
            .ReturnsAsync(() => null);
        _mockProductsRepository.Setup(repository => repository.GetProductsPage(page,size))
            .ReturnsAsync(() => new() { new ProductShortInfoDto(1, "Product") });

        var foundedPage = await _productsService.GetProductsPage(page, size);

        Assert.That(foundedPage.Count, Is.EqualTo(1));
        Assert.That(foundedPage[0].Id, Is.EqualTo(1));
        _mockCacheProvider.Verify(provider => provider.GetAsync<List<ProductShortInfoDto>>($"products:page:{page}:size:{size}"), Times.Once);
        _mockProductsRepository.Verify(repository => repository.GetProductsPage(page,size), Times.Once);
        _mockCacheProvider.Verify(provider => provider.SetAsync($"products:page:{page}:size:{size}", foundedPage, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Test]
    public void GetProductsPage_NotFound()
    {
        int page = 1, size = 10;
        _mockCacheProvider.Setup(provider => provider.GetAsync<List<ProductShortInfoDto>>($"products:page:{page}:size:{size}"))
            .ReturnsAsync(() => null);
        _mockProductsRepository.Setup(repository => repository.GetProductsPage(page, size))
            .ReturnsAsync(() => new List<ProductShortInfoDto>());

        Assert.ThrowsAsync<KeyNotFoundException>(async()=> await _productsService.GetProductsPage(page, size));

        _mockCacheProvider.Verify(provider => provider.GetAsync<List<ProductShortInfoDto>>($"products:page:{page}:size:{size}"), Times.Once);
        _mockProductsRepository.Verify(repository => repository.GetProductsPage(page, size), Times.Once);
        _mockCacheProvider.Verify(provider => provider.SetAsync($"products:page:{page}:size:{size}", It.IsAny<List<ProductShortInfoDto>>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Test] 
    public async Task GetProductsByCategory_FoundInCache()
    {
        string category = "pizza";
        string key = $"products:category:{category}";
        _mockCacheProvider.Setup(provider=>provider.GetAsync<List<ProductShortInfoDto>>(key))
            .ReturnsAsync(new List<ProductShortInfoDto>() { new ProductShortInfoDto(1,"Pizza")});

        var foundedCategory=await _productsService.GetProductsByCategory(category);

        Assert.That(foundedCategory.Count,Is.EqualTo(1));
        Assert.That(foundedCategory[0].Id, Is.EqualTo(1));
        _mockCacheProvider.Verify(provider => provider.GetAsync<List<ProductShortInfoDto>>(key), Times.Once);
        _mockProductsRepository.Verify(repository => repository.GetProductsByCategory(It.IsAny<string>()), Times.Never);
        _mockCacheProvider.Verify(provider => provider.SetAsync(It.IsAny<string>(), It.IsAny<List<ProductShortInfoDto>>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Test]
    public async Task GetProductsByCategory_FoundInRepository()
    {
        string category = "pizza";
        string key = $"products:category:{category}";
        _mockCacheProvider.Setup(provider => provider.GetAsync<List<ProductShortInfoDto>>(key))
            .ReturnsAsync(()=>null);
        _mockProductsRepository.Setup(repository => repository.GetProductsByCategory(category))
            .ReturnsAsync(new List<ProductShortInfoDto>() { new ProductShortInfoDto(1, "Pizza") });

        var foundedCategory = await _productsService.GetProductsByCategory(category);

        Assert.That(foundedCategory.Count, Is.EqualTo(1));
        Assert.That(foundedCategory[0].Id, Is.EqualTo(1));
        _mockCacheProvider.Verify(provider => provider.GetAsync<List<ProductShortInfoDto>>(key), Times.Once);
        _mockProductsRepository.Verify(repository => repository.GetProductsByCategory(category), Times.Once);
        _mockCacheProvider.Verify(provider => provider.SetAsync(key, foundedCategory, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Test]
    public void GetProductsByCategory_NotFound()
    {
        string category = "pizza";
        string key = $"products:category:{category}";
        _mockCacheProvider.Setup(provider => provider.GetAsync<List<ProductShortInfoDto>>(key))
            .ReturnsAsync(() => null);
        _mockProductsRepository.Setup(repository => repository.GetProductsByCategory(category))
            .ReturnsAsync(() => null);

        Assert.ThrowsAsync<KeyNotFoundException>(async()=> await _productsService.GetProductsByCategory(category));

        _mockCacheProvider.Verify(provider => provider.GetAsync<List<ProductShortInfoDto>>(key), Times.Once);
        _mockProductsRepository.Verify(repository => repository.GetProductsByCategory(category), Times.Once);
        _mockCacheProvider.Verify(provider => provider.SetAsync(key, It.IsAny<List<ProductShortInfoDto>>(), It.IsAny<TimeSpan>()), Times.Never);
    }
}
