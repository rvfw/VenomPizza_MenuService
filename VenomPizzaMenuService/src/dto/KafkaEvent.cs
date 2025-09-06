namespace VenomPizzaMenuService.src.dto;

public class KafkaEvent<T>
{
    public string EventType { get; set; }
    public int Id { get; set; }
    public T? Data { get; set; }
    public KafkaEvent(string eventType, T? data)
    {
        EventType = eventType;
        Data = data;
    }
}
