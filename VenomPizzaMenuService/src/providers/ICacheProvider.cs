namespace VenomPizzaMenuService.src.cache;

public interface ICacheProvider
{
    Task<T?> GetAsync<T>(string key);
    Task<Dictionary<string, T>> GetBatchAsync<T>(IEnumerable<string> ids);
    Task SetAsync(string key, object value, TimeSpan expiration);
    Task<bool> RemoveAsync<T>(string key);
}