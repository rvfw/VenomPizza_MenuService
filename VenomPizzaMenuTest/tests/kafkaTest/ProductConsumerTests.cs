using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuTest.tests.kafkaTest;

public class ProductConsumerTests
{
    private Mock<IServiceProvider> _mockServiceProvider;
    private Mock<IOptions<KafkaSettings>> _mockOptions;
    private Mock<ILogger<KafkaConsumerService>> _mockLogger;
    private Mock<IProductsService> _mockProductsService;
    private KafkaConsumerService consumerService;

    [SetUp]
    public void Setup()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockOptions = new Mock<IOptions<KafkaSettings>>();
        _mockLogger = new Mock<ILogger<KafkaConsumerService>>();
        _mockProductsService = new Mock<IProductsService>();
        var kafkaSettings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topics = new KafkaTopics
            {
                ProductCreated = "product-created",
                ProductUpdated = "product-updated",
                ProductDeleted = "product-deleted"
            }
        };
        _mockOptions.Setup(o => o.Value).Returns(kafkaSettings);
        consumerService=new KafkaConsumerService(_mockOptions.Object,_mockLogger.Object,_mockServiceProvider.Object);
    }
    [TearDown]
    public void TearDown() {
        consumerService.Dispose();
    }
    [Test]
    public async Task ProductCreatedTopic_Success()
    {
        var topic = "product-created";
        var message = """ {"Id":1,"Title":"Product"} """;
        _mockProductsService.Setup(s => s.AddProduct(new ProductDto(1, "Product"))).ReturnsAsync(new Product(1, "Product"));

        await consumerService.ProccessRequestAsync(_mockProductsService.Object, topic, message);

        _mockProductsService.Verify(s=>s.AddProduct(It.Is<ProductDto>(dto=>
        dto.Id==1 && dto.Title=="Product")), Times.Once);
    }
    [Test]
    public async Task ProductUpdatedTopic_Success()
    {
        var topic = "product-updated";
        var message = """ {"Id":1,"Title":"UpdatedProduct"} """;
        _mockProductsService.Setup(s => s.UpdateProductInfo(new ProductDto(1, "UpdatedProduct"))).ReturnsAsync(new Product(1, "UpdatedProduct"));

        await consumerService.ProccessRequestAsync(_mockProductsService.Object, topic, message);

        _mockProductsService.Verify(s => s.UpdateProductInfo(It.Is<ProductDto>(dto =>
        dto.Id == 1 && dto.Title == "UpdatedProduct")), Times.Once);
    }
    [Test]
    public async Task ProductDeletedTopic_Success()
    {
        var topic = "product-deleted";
        var message = "1";
        _mockProductsService.Setup(s => s.DeleteProductById(1));

        await consumerService.ProccessRequestAsync(_mockProductsService.Object, topic, message);

        _mockProductsService.Verify(s => s.DeleteProductById(It.Is<int>(x=>x==1)), Times.Once);
    }
}
