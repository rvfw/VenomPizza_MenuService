namespace VenomPizzaMenuService.src.kafka;

public class KafkaSettings
{
    public string BootstrapServers { get; set; }
    public string GroupId { get; set; }
    public KafkaTopics Topics { get; set; }
}
public class KafkaTopics
{
    public string ProductCreated { get; set; }
    public string ProductUpdated { get; set; }
    public string ProductDeleted { get; set; }
}
