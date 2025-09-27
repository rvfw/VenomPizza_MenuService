using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text.Json;
using VenomPizzaMenuService.src.cache;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuService.src.service;

public class ProductsService:IProductsService
{
    private readonly IProductsRepository _productsRepository;
    private readonly IProducer<string, string> _producer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<IProductsService> _logger;
    private readonly ICacheProvider _cacheProvider;

    private readonly TimeSpan productExpiration=TimeSpan.FromHours(6);
    private readonly TimeSpan categoryExpiration = TimeSpan.FromHours(1);
    private readonly TimeSpan pageExpiration = TimeSpan.FromMinutes(15);

    public ProductsService(IProductsRepository productsRepository,IProducer<string,string> producer,IOptions<KafkaSettings> settings, ILogger<IProductsService> logger, ICacheProvider cacheProvider)
    {
        _productsRepository = productsRepository;
        _kafkaSettings = settings.Value;
        _producer = producer;
        _logger = logger;
        _cacheProvider = cacheProvider;
    }

    public async Task<Product> GetProductById(int id)
    {
        var cachedProduct=await _cacheProvider.GetAsync<Product>($"product:{id}");
        if (cachedProduct != null)
            return cachedProduct;
        var foundedProduct=await _productsRepository.GetProductById(id);
        await _cacheProvider.SetAsync<Product>($"product:{id}", foundedProduct,productExpiration);
        return foundedProduct;
    }

    public async Task<List<ProductShortInfoDto>> GetProductsPage(int page, int size)
    {
        var cachedPage =await _cacheProvider.GetAsync<List<ProductShortInfoDto>>($"products:page:{page}:size:{size}");
        if (cachedPage != null)
            return cachedPage;
        var foundedPage = await _productsRepository.GetProductsPage(page, size);
        if (foundedPage == null || foundedPage.Count == 0)
            throw new KeyNotFoundException($"Страница {page} размером {size} не найдена");
        await _cacheProvider.SetAsync($"products:page:{page}:size:{size}", foundedPage, pageExpiration);
        return foundedPage;
    }

    public async Task<List<ProductShortInfoDto>> GetProductsByCategory(string categoryName)
    {
        string key = $"products:category:{categoryName}";
        var cachedCategory = await _cacheProvider.GetAsync<List<ProductShortInfoDto>>(key);
        if (cachedCategory != null)
            return cachedCategory;
        var foundedProducts=await _productsRepository.GetProductsByCategory(categoryName);
        if (foundedProducts == null || foundedProducts.Count == 0)
            throw new KeyNotFoundException($"Категория {categoryName} не найдена");
        await _cacheProvider.SetAsync<List<ProductShortInfoDto>>(key, foundedProducts,categoryExpiration);
        return foundedProducts;
    }

    #region productUpdate
    public async Task<Product> AddProduct(ProductDto newProduct)
    {
        newProduct.Validate();
        Product result;
        if (newProduct is ComboDto newCombo)
        {
            await _productsRepository.CheckComboProducts(newCombo);
            result = await _productsRepository.AddProduct(newCombo);
        }
        else if (newProduct is DishDto newDish)
            result = await _productsRepository.AddProduct(newDish);
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
        await _cacheProvider.RemoveAsync<Product>(result.Id.ToString());
        return result;
    }

    public async Task DeleteProductById(int id)
    {
        var foundedProduct = await _productsRepository.GetProductIdAndTitle(id,0);
        if (foundedProduct == null)
            throw new KeyNotFoundException($"Продукт с Id {id} не найден");
        var result=await _productsRepository.DeleteProductById(id);
        if (!result)
            throw new KeyNotFoundException($"Продукт с Id {id} не найден");
        await SendInProductUpdatedTopic("product_deleted", new ProductShortInfoDto(foundedProduct.Value.id, foundedProduct.Value.title));
        await _cacheProvider.RemoveAsync<Product>(foundedProduct.Value.id.ToString());
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
            throw new KeyNotFoundException($"Цена {priceId} для продукта {id} не найдена");
        await SendInCartUpdatedTopic("product_updated", cartId, id, priceId, quantity);
    }

    public async Task DeleteProductInCart(int cartId, int id, int priceId)
    {
        if (_productsRepository.GetProductIdAndTitle(id, priceId) == null)
            throw new KeyNotFoundException($"Продукт {id} с ценой {priceId} не найден");
        await SendInCartUpdatedTopic("product_deleted", cartId, id, priceId);
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
