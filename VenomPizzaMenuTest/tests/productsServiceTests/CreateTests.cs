using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VenomPizzaMenuService.src.cache;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuTest.tests.productServiceTests;

[TestFixture]
internal class CreateTests
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
    public async Task AddProduct_Success()
    {
        var productDto = new ProductDto(1, "Product");
        _mockProductsRepository.Setup(repository => repository.GetProductById(1))
            .ReturnsAsync(()=>null);
        _mockProductsRepository.Setup(repository=>repository.AddProduct(productDto))
            .ReturnsAsync(new Product(productDto));
        _mockProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(),It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        var result =await _productsService.AddProduct(productDto);

        Assert.That(result.GetType(), Is.EqualTo(typeof(Product)));
        Assert.That(result.Id, Is.EqualTo(productDto.Id));
        Assert.That(result.Title, Is.EqualTo(productDto.Title));
        _mockProductsRepository.Verify(repository => repository.GetProductById(1), Times.Once);
        _mockProductsRepository.Verify(repository => repository.AddProduct(productDto), Times.Once);
        _mockProducer.Verify(producer => producer.ProduceAsync(_kafkaSettings.Topics.ProductUpdated,It.IsAny<Message<string,string>>(),It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AddDish_Success()
    {
        var DishDto = new DishDto(1, "Dish") {Fats=100 };
        _mockProductsRepository.Setup(repository => repository.GetProductById(1))
            .ReturnsAsync(() => null);
        _mockProductsRepository.Setup(repository => repository.AddProduct(DishDto))
            .ReturnsAsync(new Dish(DishDto));
        _mockProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        var result = await _productsService.AddProduct(DishDto);

        Assert.That(result.GetType(), Is.EqualTo(typeof(Dish)));
        Assert.That(result.Id, Is.EqualTo(DishDto.Id));
        Assert.That(result.Title, Is.EqualTo(DishDto.Title));
        Assert.That(((Dish)result).Fats, Is.EqualTo(DishDto.Fats));
        _mockProductsRepository.Verify(repository => repository.GetProductById(1), Times.Once);
        _mockProductsRepository.Verify(repository => repository.AddProduct(DishDto), Times.Once);
        _mockProducer.Verify(producer => producer.ProduceAsync(_kafkaSettings.Topics.ProductUpdated, It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AddCombo_Success()
    {
        var comboDto = new ComboDto(1, "Combo") { ProductsDict = new() { [2]=5 } };
        _mockProductsRepository.Setup(repository => repository.GetProductById(1))
            .ReturnsAsync(() => null);
        _mockProductsRepository.Setup(repository => repository.AddProduct(comboDto))
            .ReturnsAsync(new Combo(comboDto));
        _mockProductsRepository.Setup(repository => repository.AllComboProductsExist(comboDto))
            .ReturnsAsync(true);
        _mockProductsRepository.Setup(repository => repository.ComboContainsCombo(comboDto))
            .ReturnsAsync(false);
        _mockProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        var result = await _productsService.AddProduct(comboDto);

        Assert.That(result.GetType(), Is.EqualTo(typeof(Combo)));
        Assert.That(result.Id, Is.EqualTo(comboDto.Id));
        Assert.That(result.Title, Is.EqualTo(comboDto.Title));
        _mockProductsRepository.Verify(repository => repository.GetProductById(1), Times.Once);
        _mockProductsRepository.Verify(repository => repository.AddProduct(comboDto), Times.Once);
        _mockProductsRepository.Verify(repository => repository.AllComboProductsExist(comboDto), Times.Once);
        _mockProductsRepository.Verify(repository => repository.ComboContainsCombo(comboDto), Times.Once);
        _mockProducer.Verify(producer => producer.ProduceAsync(_kafkaSettings.Topics.ProductUpdated, It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void AddCombo_ProductNotFound()
    {
        var comboDto = new ComboDto(1, "Combo") { ProductsDict = new() { [2] = 5 } };
        _mockProductsRepository.Setup(repository => repository.GetProductById(1))
            .ReturnsAsync(() => null);
        _mockProductsRepository.Setup(repository => repository.AddProduct(comboDto))
            .ReturnsAsync(new Combo(comboDto));
        _mockProductsRepository.Setup(repository => repository.AllComboProductsExist(comboDto))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<ArgumentException>(async () => await _productsService.AddProduct(comboDto));
    }

    [Test]
    public void AddCombo_ComboInCombo()
    {
        var comboDto = new ComboDto(1, "Combo") { ProductsDict = new() { [2] = 5 } };
        _mockProductsRepository.Setup(repository => repository.GetProductById(1))
            .ReturnsAsync(() => null);
        _mockProductsRepository.Setup(repository => repository.AddProduct(comboDto))
            .ReturnsAsync(new Combo(comboDto));
        _mockProductsRepository.Setup(repository => repository.ComboContainsCombo(comboDto))
            .ReturnsAsync(true);

        Assert.ThrowsAsync<ArgumentException>(async () => await _productsService.AddProduct(comboDto));
    }

    [Test]
    public void AddProduct_AlreadyExist()
    {
        var productDto = new ProductDto(1, "Product");
        _mockProductsRepository.Setup(repository => repository.GetProductById(1))
            .ReturnsAsync(() => new Product(1,"Product"));

        Assert.ThrowsAsync<ArgumentException>(async()=>await _productsService.AddProduct(productDto), "Товар с Id 1 уже существует");
    }

}
