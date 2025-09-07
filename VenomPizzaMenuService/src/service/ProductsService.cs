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

    #region create
    public async Task<Product> AddProduct(ProductDto newProduct)
    {
        newProduct.Validate();
        Product result;
        if (newProduct is ComboDto)
            result= await _productsRepository.AddProduct((ComboDto)newProduct);
        else if(newProduct is DishDto)
            result= await _productsRepository.AddProduct((DishDto)newProduct);
        else
            result= await _productsRepository.AddProduct(newProduct);
        await SendInProductUpdatedTopic("product_added", new ProductShortInfoDto(result));
        return result;
    }

    public async Task AddProductToCart(int cartId,int id,int quantity)
    {
        var foundedProduct = await _productsRepository.GetProductById(id);
        if (foundedProduct == null)
            throw new KeyNotFoundException($"Продукта в Id {id} не найдено");
        if (!foundedProduct.IsAvailable)
            throw new BadHttpRequestException($"Продукт {foundedProduct.Title} с Id {foundedProduct.Id} не доступен для заказа на данный момент");
        await SendInCartUpdatedTopic("product_added",cartId, id, quantity);
    }
    #endregion

    #region read
    public async Task<Product> GetProductById(int id)
    {
        return await _productsRepository.GetProductById(id);
    }

    public async Task<List<ProductShortInfoDto>> GetProductsPage(int page,int size)
    {
        return await _productsRepository.GetProductsPage(page,size);
    }
    #endregion

    #region update
    public async Task<Product> UpdateProductInfo(ProductDto updatedProduct)
    {
        updatedProduct.Validate();
        var result= await _productsRepository.UpdateProductInfo(updatedProduct);
        await SendInProductUpdatedTopic("product_updated", new ProductShortInfoDto(result));
        return result;
    }

    public async Task UpdateProductQuantityInCart(int cartId, int id, int quantity)
    {
        var foundedProduct= await _productsRepository.GetProductById(id);
        if (foundedProduct == null)
            throw new KeyNotFoundException($"Продукта в Id {id} не найдено");
        await SendInCartUpdatedTopic("product_updated",cartId, id, quantity);
    }
    #endregion

    #region delete
    public async Task DeleteProductById(int id)
    {
        var foundedProduct=await _productsRepository.GetProductIdAndTitle(id);
        await _productsRepository.DeleteProductById(id);
        await SendInProductUpdatedTopic("product_deleted", new ProductShortInfoDto(foundedProduct.Value.Id,foundedProduct.Value.Title));
    }

    public async Task DeleteProductInCart(int cartId, int id)
    {
        var foundedProduct=await _productsRepository.GetProductIdAndTitle(id);
        if (foundedProduct == null)
            throw new KeyNotFoundException($"Продукта в Id {id} не найдено");
        await SendInCartUpdatedTopic("product_deleted",cartId, id);
    }
    #endregion

    #region private
    private async Task SendInCartUpdatedTopic(string eventType, int cartId, int id, int quantity = 1)
    {
        var kafkaEvent = new KafkaEvent<CartProductDto>(eventType, new CartProductDto(cartId, id, quantity));
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
        _logger.LogInformation($"Отправлено {kafkaMessage.Value} в {_kafkaSettings.Topics.CartUpdated}");
    }
    #endregion
}
