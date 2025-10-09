using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Constraints;
using VenomPizzaMenuService.src.cache;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuTest.tests.productServiceTests;

[TestFixture]
internal class UpdateTests
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
    public async Task UpdateProductInfo_Success()
    {
        var updatedProduct = new ProductDto(1, "UpdatedProduct");
        _mockProductsRepository.Setup(repository => repository.UpdateProductInfo(updatedProduct))
            .ReturnsAsync(new Product(updatedProduct));
        _mockProducer.Setup(producer => producer.ProduceAsync(_kafkaSettings.Topics.ProductUpdated, It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        var result = await _productsService.UpdateProductInfo(updatedProduct);

        Assert.That(result.Id, Is.EqualTo(updatedProduct.Id));
        Assert.That(result.Title, Is.EqualTo(updatedProduct.Title));
        _mockProductsRepository.Verify(repository => repository.UpdateProductInfo(updatedProduct), Times.Once);
        _mockCacheProvider.Verify(provider => provider.RemoveAsync<Product>($"product:{updatedProduct.Id}"),Times.Once);
        _mockProducer.Verify(producer => producer.ProduceAsync(_kafkaSettings.Topics.ProductUpdated, 
            It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
