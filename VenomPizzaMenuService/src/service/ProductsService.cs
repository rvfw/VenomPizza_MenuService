using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuService.src.service;

public class ProductsService:IProductsService
{
    private readonly ProductsRepository productsRepository;
    private readonly IProducer<string, string> _producer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<IProductsService> _logger;
    public ProductsService(ProductsRepository productsRepository,IProducer<string,string> producer,IOptions<KafkaSettings> settings, ILogger<IProductsService> logger)
    {
        this.productsRepository = productsRepository;
        _kafkaSettings = settings.Value;
        _producer = producer;
        _logger = logger;
    }

    #region create
    public async Task<Product> AddProduct(ProductDto newProduct)
    {
        newProduct.Validate();
        if (newProduct is ComboDto)
            return await productsRepository.AddProduct((ComboDto)newProduct);
        else if(newProduct is DishDto)
            return await productsRepository.AddProduct((DishDto)newProduct);
        else
            return await productsRepository.AddProduct(newProduct);
    }

    public async Task AddProductToCart(int cartId,int id,int quantity)
    {
        var kafkaMessage = new Message<string, string>
        {
            Key = cartId.ToString(),
            Value = JsonSerializer.Serialize(new CartProductDto(cartId, id, quantity))
        };
        await _producer.ProduceAsync(_kafkaSettings.Topics.ProductAddedInCart, kafkaMessage);
        _logger.LogInformation($"Отправлено {kafkaMessage} в {_kafkaSettings.Topics.ProductAddedInCart}");
    }
    #endregion

    #region read
    public async Task<Product> GetProductById(int id)
    {
        return await productsRepository.GetProductById(id);
    }
    public async Task<List<ProductInMenuDto>> GetProductsPage(int page,int size)
    {
        return await productsRepository.GetProductsPage(page,size);
    }
    #endregion

    #region update
    public async Task<Product> UpdateProductInfo(ProductDto updatedProduct)
    {
        updatedProduct.Validate();
        return await productsRepository.UpdateProductInfo(updatedProduct);
    }

    public async Task UpdateProductQuantityInCart(int cartId, int id, int quantity)
    {
        var kafkaMessage = new Message<string, string>
        {
            Key = cartId.ToString(),
            Value = JsonSerializer.Serialize(new CartProductDto(cartId, id, quantity))
        };
        await _producer.ProduceAsync(_kafkaSettings.Topics.ProductQuantityUpdated, kafkaMessage);
        _logger.LogInformation($"Отправлено {kafkaMessage} в {_kafkaSettings.Topics.ProductQuantityUpdated}");
    }
    #endregion

    #region delete
    public async Task DeleteProductById(int id)
    {
        await productsRepository.DeleteProductById(id);
    }

    public async Task DeleteProductInCart(int cartId, int id)
    {
        var kafkaMessage = new Message<string, string>
        {
            Key = cartId.ToString(),
            Value = JsonSerializer.Serialize(new CartProductDto(cartId, id))
        };
        await _producer.ProduceAsync(_kafkaSettings.Topics.ProductDeletedInCart, kafkaMessage);
        _logger.LogInformation($"Отправлено {kafkaMessage} в {_kafkaSettings.Topics.ProductDeletedInCart}");
    }
    #endregion
}
