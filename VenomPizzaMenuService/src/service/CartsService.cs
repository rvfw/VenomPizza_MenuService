using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuService.src.service;

public class CartsService:ICartsService
{
    private readonly IProductsRepository _productsRepository;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<CartsService> _logger;
    private readonly IProducer<string, string> _producer;

    public CartsService(IProductsRepository productsRepository, IProducer<string, string> producer, IOptions<KafkaSettings> settings, ILogger<CartsService> logger)
    {
        _productsRepository = productsRepository;
        _kafkaSettings = settings.Value;
        _producer = producer;
        _logger = logger;
    }
    public async Task AddProductToCart(int cartId, int id, int priceId, int quantity)
    {
        var foundedProduct = await _productsRepository.GetProductById(id);
        if (foundedProduct == null)
            throw new KeyNotFoundException($"Продукта c Id {id} не найдено");
        if (foundedProduct.PriceVariants.FirstOrDefault(x => x.PriceId == priceId) == null)
            throw new KeyNotFoundException($"Цена размера с Id {priceId} для продукта {id} не найдена");
        if (!foundedProduct.IsAvailable)
            throw new BadHttpRequestException($"Продукт {foundedProduct.Title} с Id {foundedProduct.Id} не доступен для заказа на данный момент");
        await SendInCartUpdatedTopic("product_added", cartId, id, priceId, quantity);
    }

    public async Task UpdateProductQuantityInCart(int cartId, int id, int priceId, int quantity)
    {
        var foundedProduct = await _productsRepository.GetProductById(id);
        if (foundedProduct == null)
            throw new KeyNotFoundException($"Продукта в Id {id} не найдено");
        if (foundedProduct.PriceVariants.FirstOrDefault(x => x.PriceId == priceId) == null)
            throw new KeyNotFoundException($"Цена {priceId} для продукта {id} не найдена");
        await SendInCartUpdatedTopic("product_updated", cartId, id, priceId, quantity);
    }

    public async Task DeleteProductInCart(int cartId, int id, int priceId)
    {
        if (_productsRepository.GetProductIdAndTitle(id, priceId) == null)
            throw new KeyNotFoundException($"Продукт {id} с ценой {priceId} не найден");
        await SendInCartUpdatedTopic("product_deleted", cartId, id, priceId);
    }

    private async Task SendInCartUpdatedTopic(string eventType, int cartId, int id, int priceId, int quantity = 1)
    {
        var kafkaEvent = new KafkaEvent<CartProductDto>(eventType, new CartProductDto(cartId, id, priceId, quantity));
        var kafkaMessage = new Message<string, string>
        {
            Key = cartId.ToString(),
            Value = JsonSerializer.Serialize(kafkaEvent)
        };
        await _producer.ProduceAsync(_kafkaSettings.Topics.CartUpdated, kafkaMessage);
        _logger.LogInformation($"Отправлено {kafkaMessage.Value} в {_kafkaSettings.Topics.CartUpdated}");
    }
}
