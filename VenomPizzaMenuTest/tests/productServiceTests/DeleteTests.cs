using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VenomPizzaMenuService.src.cache;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuTest.tests.productServiceTests;

[TestFixture]
internal class DeleteTests
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
    public async Task DeleteProductById_Success()
    {
        int id = 1;
        _mockProductsRepository.Setup(repository => repository.DeleteProductById(id))
            .ReturnsAsync(true);
        _mockCacheProvider.Setup(provider => provider.RemoveAsync<Product>($"product:{id}"))
            .ReturnsAsync(true);
        _mockProducer.Setup(producer => producer.ProduceAsync(_kafkaSettings.Topics.ProductUpdated, It.IsAny<Message<string, string>>(),It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        await _productsService.DeleteProductById(id);

        _mockProductsRepository.Verify(repository => repository.DeleteProductById(id), Times.Once);
        _mockCacheProvider.Verify(provider => provider.RemoveAsync<Product>($"product:{id}"), Times.Once);
        _mockProducer.Verify(producer => producer.ProduceAsync(_kafkaSettings.Topics.ProductUpdated, 
            It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()),Times.Once);
    }

    [Test]
    public void DeleteProductById_NotFound()
    {
        int id = 1;
        _mockProductsRepository.Setup(repository => repository.DeleteProductById(id))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<KeyNotFoundException>(async ()=> await _productsService.DeleteProductById(id));

        _mockProductsRepository.Verify(repository => repository.DeleteProductById(id), Times.Once);
        _mockCacheProvider.Verify(provider => provider.RemoveAsync<Product>($"product:{id}"), Times.Never);
        _mockProducer.Verify(producer => producer.ProduceAsync(_kafkaSettings.Topics.ProductUpdated,
            It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
