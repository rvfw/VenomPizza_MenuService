using StackExchange.Redis;
using System.Text.Json;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.cache;

public class CacheProvider : ICacheProvider
{
    private readonly IDatabase _database;
    private readonly ILogger<CacheProvider> _logger;
    private static readonly Dictionary<Type, string> _keyPrefix = new()
    {
        [typeof(Product)]="product",
        [typeof(List<ProductShortInfoDto>)]="products:page"
    };

    public CacheProvider(IConnectionMultiplexer redis, ILogger<CacheProvider> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var fullKey = GetKey(typeof(T), key);
            var foundedValue = await _database.StringGetAsync(key);
            if (foundedValue.IsNullOrEmpty)
                return default;
            _logger.LogInformation($"Получено значение из редиса {key}");
            return JsonSerializer.Deserialize<T>(foundedValue!);
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
            var keys = ids.Select(x =>(RedisKey)GetKey(typeof(T), x.ToString())).ToArray();
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

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        try
        {
            var fullKey = GetKey(typeof(T), key);
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiration);
            _logger.LogInformation($"Установлено значение в редисе для {fullKey}");
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
            var fullKey = GetKey(typeof(T), key);
            var res = await _database.KeyDeleteAsync(key);
            if (res)
                _logger.LogInformation($"Удалено значение в редисе: {fullKey}");
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Не удалось удалить в редисе значение для ключа {key}: {ex.Message}");
            return false;
        }
    }
    private string GetKey(Type type,string key)
    {
        if (!_keyPrefix.TryGetValue(type, out var prefix))
            throw new KeyNotFoundException($"Не найден префикс для редиса для типа {type}");
        return $"{prefix}:{key}";
    }
}