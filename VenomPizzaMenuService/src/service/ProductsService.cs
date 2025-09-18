using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuService.src.service;

public class ProductsService:IProductsService
{
    private readonly ProductsRepository _productsRepository;
    private readonly IProducer<string, string> _producer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<IProductsService> _logger;

    public ProductsService(ProductsRepository productsRepository,IProducer<string,string> producer,IOptions<KafkaSettings> settings, ILogger<IProductsService> logger)
    {
        this._productsRepository = productsRepository;
        _kafkaSettings = settings.Value;
        _producer = producer;
        _logger = logger;
    }

    public async Task<Product> GetProductById(int id)
    {
        return await _productsRepository.GetProductById(id);
    }

    public async Task<List<ProductShortInfoDto>> GetProductsPage(int page, int size)
    {
        return await _productsRepository.GetProductsPage(page, size);
    }

    #region productUpdate
    public async Task<Product> AddProduct(ProductDto newProduct)
    {
        newProduct.Validate();
        Product result;
        if (newProduct is ComboDto)
            result = await _productsRepository.AddProduct((ComboDto)newProduct);
        else if (newProduct is DishDto)
            result = await _productsRepository.AddProduct((DishDto)newProduct);
        else
            result = await _productsRepository.AddProduct(newProduct);
        await SendInProductUpdatedTopic("product_added", new ProductShortInfoDto(result));
        return result;
    }

    public async Task<Product> UpdateProductInfo(ProductDto updatedProduct)
    {
        updatedProduct.Validate();
        var result = await _productsRepository.UpdateProductInfo(updatedProduct);
        await SendInProductUpdatedTopic("product_updated", new ProductShortInfoDto(result));
        return result;
    }

    public async Task DeleteProductById(int id)
    {
        var foundedProduct = await _productsRepository.GetProductIdAndTitle(id,0);
        await _productsRepository.DeleteProductById(id);
        await SendInProductUpdatedTopic("product_deleted", new ProductShortInfoDto(foundedProduct.id, foundedProduct.title));
    }
    #endregion

    #region cartUpdate
    public async Task AddProductToCart(int cartId, int id,int priceId, int quantity)
    {
        var foundedProduct = await _productsRepository.GetProductById(id);
        if (foundedProduct == null)
            throw new KeyNotFoundException($"Продукта c Id {id} не найдено");
        if (foundedProduct.PriceVariants.FirstOrDefault(x => x.PriceId == priceId) == null)
            throw new KeyNotFoundException($"Цена размера с Id {priceId} для продукта {id} не найдена");
        if (!foundedProduct.IsAvailable)
            throw new BadHttpRequestException($"Продукт {foundedProduct.Title} с Id {foundedProduct.Id} не доступен для заказа на данный момент");
        await SendInCartUpdatedTopic("product_added", cartId, id,priceId, quantity);
    }

    public async Task UpdateProductQuantityInCart(int cartId, int id,int priceId, int quantity)
    {
        var foundedProduct = await _productsRepository.GetProductById(id);
        if (foundedProduct == null)
            throw new KeyNotFoundException($"Продукта в Id {id} не найдено");
        if (foundedProduct.PriceVariants.FirstOrDefault(x => x.PriceId == priceId) == null)
            throw new KeyNotFoundException($"Цена размера c Id {priceId} для продукта {id} не найдена");
        await SendInCartUpdatedTopic("product_updated", cartId, id, priceId, quantity);
    }

    public async Task DeleteProductInCart(int cartId, int id,int priceId)
    {
        var foundedProduct = await _productsRepository.GetProductIdAndTitle(id,priceId);
        await SendInCartUpdatedTopic("product_deleted", cartId, id,priceId);
    } 
    #endregion


    #region private
    private async Task SendInCartUpdatedTopic(string eventType, int cartId, int id,int priceId, int quantity = 1)
    {
        var kafkaEvent = new KafkaEvent<CartProductDto>(eventType, new CartProductDto(cartId, id,priceId, quantity));
        var kafkaMessage = new Message<string, string>
        {
            Key = cartId.ToString(),
            Value = JsonSerializer.Serialize(kafkaEvent)
        };
        await _producer.ProduceAsync(_kafkaSettings.Topics.CartUpdated, kafkaMessage);
        _logger.LogInformation($"Отправлено {kafkaMessage.Value} в {_kafkaSettings.Topics.CartUpdated}");
    }

    private async Task SendInProductUpdatedTopic(string eventType, ProductShortInfoDto product)
    {
        var kafkaEvent = new KafkaEvent<ProductShortInfoDto>(eventType, product);
        var kafkaMessage = new Message<string, string>
        {
            Key = product.Id.ToString(),
            Value = JsonSerializer.Serialize(kafkaEvent)
        };
        await _producer.ProduceAsync(_kafkaSettings.Topics.ProductUpdated, kafkaMessage);
        _logger.LogInformation($"Отправлено {kafkaMessage.Value} в {_kafkaSettings.Topics.ProductUpdated}");
    }
    #endregion
}
