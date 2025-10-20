using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.cache;

public class CacheProvider : ICacheProvider
{
    private readonly IDatabase _database;
    private readonly ILogger<CacheProvider> _logger;

    public CacheProvider(IConnectionMultiplexer redis, ILogger<CacheProvider> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var foundedObject = await _database.StringGetAsync(key);
            if (foundedObject.IsNullOrEmpty)
                return default;
            _logger.LogInformation($"Получено значение из редиса {key}");
            var redisProductDto= JsonSerializer.Deserialize<RedisProductDto>(foundedObject!);
            Type type = redisProductDto.Type switch
            {
                "Dish" => typeof(Dish),
                "Combo" => typeof(Combo),
                "Product" => typeof(Product),
                _ => throw new KeyNotFoundException("Неверный тип продукта"),
            };
            return (T)redisProductDto.Value.Deserialize(type);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Не удалось получить в редисе значение с ключом {key}: {ex.Message}");
            return default;
        }
    }

    public async Task<Dictionary<string,T>> GetBatchAsync<T>(IEnumerable<string> ids)
    {
        try
        {
            var keys = ids.Select(x =>(RedisKey)x.ToString()).ToArray();
            var values = await _database.StringGetAsync(keys);
            var result= new Dictionary<string,T>();
            for(int i=0; i<keys.Length; i++)
                if (!values[i].IsNull)
                    result[ids.ElementAt(i)]=JsonSerializer.Deserialize<T>(values[i]!)!;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Не удалось получить в редисе значения:\n {ex.Message}");
            return new Dictionary<string, T>();
        }
    }

    public async Task SetAsync(string key, object value, TimeSpan expiration)
    {
        try
        {
            var redisProductDto = new RedisProductDto((Product)value);
            var json = JsonSerializer.Serialize(redisProductDto);
            await _database.StringSetAsync(key, json, expiration);
            _logger.LogInformation($"Установлено значение в редисе для {key}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Не удалось записать в редис значение с ключом {key}: {ex.Message}");
        }
    }

    public async Task<bool> RemoveAsync<T>(string key)
    {
        try
        {
            var res = await _database.KeyDeleteAsync(key);
            if (res)
                _logger.LogInformation($"Удалено значение в редисе: {key}");
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Не удалось удалить в редисе значение для ключа {key}: {ex.Message}");
            return false;
        }
    }
}