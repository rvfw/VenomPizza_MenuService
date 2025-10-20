using System.Text.Json;

namespace VenomPizzaMenuService.src.dto;

public class RedisProductDto
{
    public string Type { get; set; }
    public JsonElement Value { get; set; }
    
    public RedisProductDto(){}

    public RedisProductDto(object redisObject)
    {
        var json = JsonSerializer.Serialize(redisObject, redisObject.GetType());
        using var doc = JsonDocument.Parse(json);
        Type = redisObject.GetType().Name;
        Value = doc.RootElement.Clone();
    }
}