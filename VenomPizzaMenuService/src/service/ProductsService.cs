using Confluent.Kafka;
using Microsoft.Extensions.Options;
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

    public ProductsService(IProductsRepository productsRepository,IProducer<string,string> producer,IOptions<KafkaSettings> settings, ILogger<ProductsService> logger, ICacheProvider cacheProvider)
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

    public async Task<Product> AddProduct(ProductDto newProduct)
    {
        newProduct.Validate();
        if (await _productsRepository.GetProductById(newProduct.Id) != null)
            throw new ArgumentException($"Товар с Id {newProduct.Id} уже существует");
        Product result;
        if (newProduct is ComboDto newCombo)
        {
            if(!await _productsRepository.AllComboProductsExist(newCombo))
                throw new ArgumentException("Один из товаров комбо не найден");
            if(await _productsRepository.ComboContainsCombo(newCombo))
                throw new ArgumentException("Нельзя добавить комбо в комбо");
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
}
