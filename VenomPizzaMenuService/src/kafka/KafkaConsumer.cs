
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using System.Runtime;
using System.Text.Json;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuService.src.kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly KafkaSettings settings;
    private readonly IServiceProvider serviceProvider;
    private readonly IConsumer<string, string> consumer;
    private readonly ILogger<KafkaConsumerService> logger;
    public KafkaConsumerService(IOptions<KafkaSettings> settings,ILogger<KafkaConsumerService> logger,IServiceProvider serviceProvider)
    {
        this.logger=logger;
        this.settings=settings.Value; 
        this.serviceProvider=serviceProvider;
        var config = new ConsumerConfig
        {
            BootstrapServers = this.settings.BootstrapServers,
            GroupId = this.settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        consumer = new ConsumerBuilder<string, string>(config).Build();
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topics = new[] {
            settings.Topics.ProductCreated,
            settings.Topics.ProductUpdated,
            settings.Topics.ProductDeleted
        };
        consumer.Subscribe(topics);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                logger.LogInformation($"Получено: {result.Message.Value}");
                using (var scope = serviceProvider.CreateScope())
                {
                    var productsService = scope.ServiceProvider.GetRequiredService<ProductsService>();
                    await ProccessRequestAsync(productsService,result.Topic, result.Message.Value);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка обработки запроса");
            }
        }
        consumer.Close();
    }
    private async Task ProccessRequestAsync(ProductsService productsService,string topic,string message)
    {
        if (new List<string>() { settings.Topics.ProductCreated, settings.Topics.ProductUpdated }.Contains(topic))
        {
            ProductDto dto = JsonSerializer.Deserialize<ProductDto>(message,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            if (topic == settings.Topics.ProductCreated)
                await productsService.AddProduct(dto);
            else if (topic == settings.Topics.ProductUpdated)
                await productsService.UpdateProductInfo(dto);
        }
        else if (topic == settings.Topics.ProductDeleted)
            await productsService.DeleteProductById(int.Parse(message));
    }
}
