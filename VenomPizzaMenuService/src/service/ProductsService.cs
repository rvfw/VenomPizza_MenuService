using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SQLitePCL;
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
        if (newProduct is ComboDto)
            return await _productsRepository.AddProduct((ComboDto)newProduct);
        else if(newProduct is DishDto)
            return await _productsRepository.AddProduct((DishDto)newProduct);
        else
            return await _productsRepository.AddProduct(newProduct);
    }

    public async Task AddProductToCart(int cartId,int id,int quantity)
    {
        if (await _productsRepository.GetProductById(id) == null)
            throw new KeyNotFoundException($"Продукта в Id {id} не найдено");
        var kafkaEvent=new KafkaEvent<CartProductDto>("product_added",new CartProductDto(cartId, id, quantity));
        var kafkaMessage = new Message<string, string>
        {
            Key = cartId.ToString(),
            Value = JsonSerializer.Serialize(kafkaEvent)
        };
        await _producer.ProduceAsync(_kafkaSettings.Topics.CartUpdated, kafkaMessage);
        _logger.LogInformation($"Отправлено {kafkaMessage.Value} в {_kafkaSettings.Topics.CartUpdated}");
    }
    #endregion

    #region read
    public async Task<Product> GetProductById(int id)
    {
        return await _productsRepository.GetProductById(id);
    }

    public async Task<List<ProductInMenuDto>> GetProductsPage(int page,int size)
    {
        return await _productsRepository.GetProductsPage(page,size);
    }
    #endregion

    #region update
    public async Task<Product> UpdateProductInfo(ProductDto updatedProduct)
    {
        updatedProduct.Validate();
        return await _productsRepository.UpdateProductInfo(updatedProduct);
    }

    public async Task UpdateProductQuantityInCart(int cartId, int id, int quantity)
    {
        var kafkaEvent = new KafkaEvent<CartProductDto>("product_updated", new CartProductDto(cartId, id, quantity));
        var kafkaMessage = new Message<string, string>
        {
            Key = cartId.ToString(),
            Value = JsonSerializer.Serialize(kafkaEvent)
        };
        await _producer.ProduceAsync(_kafkaSettings.Topics.CartUpdated, kafkaMessage);
        _logger.LogInformation($"Отправлено {kafkaMessage.Value} в {_kafkaSettings.Topics.CartUpdated}");
    }
    #endregion

    #region delete
    public async Task DeleteProductById(int id)
    {
        await _productsRepository.DeleteProductById(id);
    }

    public async Task DeleteProductInCart(int cartId, int id)
    {
        var kafkaEvent = new KafkaEvent<CartProductDto>("product_deleted", new CartProductDto(cartId, id));
        var kafkaMessage = new Message<string, string>
        {
            Key = cartId.ToString(),
            Value = JsonSerializer.Serialize(kafkaEvent)
        };
        await _producer.ProduceAsync(_kafkaSettings.Topics.CartUpdated, kafkaMessage);
        _logger.LogInformation($"Отправлено {kafkaMessage.Value} в {_kafkaSettings.Topics.CartUpdated}");
    }
    #endregion
}
