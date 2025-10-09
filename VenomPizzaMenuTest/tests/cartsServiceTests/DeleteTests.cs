using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.repository;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuTest.tests.cartsServiceTests;

internal class DeleteTests
{
    private Mock<IProductsRepository> _mockProductsRepository;
    private Mock<IProducer<string, string>> _mockProducer;
    private KafkaSettings _kafkaSettings;
    private Mock<ILogger<CartsService>> _mockLogger;
    private ICartsService _cartsService;

    [SetUp]
    public void Setup()
    {
        _mockProductsRepository = new Mock<IProductsRepository>();
        _mockProducer = new Mock<IProducer<string, string>>();
        _mockLogger = new Mock<ILogger<CartsService>>();
        _kafkaSettings = new KafkaSettings
        {
            Topics = new KafkaTopics
            {
                CartUpdated = "cart-updated-topic",
            },
            BootstrapServers = "localhost:9092"
        };
        _cartsService = new CartsService(_mockProductsRepository.Object, _mockProducer.Object, Options.Create(_kafkaSettings), _mockLogger.Object);
    }

    [Test]
    public async Task DeleteProductInCart_Success()
    {
        int cartId=1, productId=2, priceId=3;
        _mockProductsRepository.Setup(repository => repository.GetProductIdAndTitle(productId, priceId))
            .ReturnsAsync((productId,"Product"));
        _mockProducer.Setup(producer => producer.ProduceAsync(_kafkaSettings.Topics.CartUpdated,
            It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        await _cartsService.DeleteProductInCart(cartId, productId,priceId);

        _mockProductsRepository.Verify(repository => repository.GetProductIdAndTitle(productId,priceId), Times.Once);
        _mockProducer.Verify(producer => producer.ProduceAsync(_kafkaSettings.Topics.CartUpdated,
            It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void DeleteProductInCart_ProductNotFound()
    {
        int cartId = 1, productId = 2, priceId = 3;
        _mockProductsRepository.Setup(repository => repository.GetProductIdAndTitle(productId, priceId))
            .ReturnsAsync(()=>null);

        Assert.ThrowsAsync<KeyNotFoundException>(async()=> await _cartsService.DeleteProductInCart(cartId, productId, priceId));

        _mockProductsRepository.Verify(repository => repository.GetProductIdAndTitle(productId, priceId), Times.Once);
        _mockProducer.Verify(producer => producer.ProduceAsync(_kafkaSettings.Topics.CartUpdated,
            It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
