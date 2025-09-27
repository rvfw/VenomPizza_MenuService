using StackExchange.Redis;
using System.Text.Json;

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

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
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