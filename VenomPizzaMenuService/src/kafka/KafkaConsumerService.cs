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
    private readonly KafkaSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumerService> _logger;
    public KafkaConsumerService(IOptions<KafkaSettings> settings,ILogger<KafkaConsumerService> logger,IServiceProvider serviceProvider)
    {
        _logger=logger;
        _settings=settings.Value; 
        _serviceProvider=serviceProvider;
        var config = new ConsumerConfig
        {
            BootstrapServers = this._settings.BootstrapServers,
            GroupId = this._settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topics = new[] {
            _settings.Topics.ProductCreated,
            _settings.Topics.ProductUpdated,
            _settings.Topics.ProductDeleted
        };
        _consumer.Subscribe(topics);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result =await Task.Run(()=> _consumer.Consume(stoppingToken),stoppingToken);
                _logger.LogInformation($"Получено из топика {result.Topic}:\n{result.Message.Value}");
                using (var scope = _serviceProvider.CreateScope())
                {
                    var productsService = scope.ServiceProvider.GetRequiredService<ProductsService>();
                    await ProccessRequestAsync(productsService,result.Topic, result.Message.Value);
                }
                _consumer.Commit(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки запроса");
                break;
            }
        }
        _consumer.Close();
    }
    public async Task ProccessRequestAsync(IProductsService productsService,string topic,string message)
    {
        if (new List<string>() { _settings.Topics.ProductCreated, _settings.Topics.ProductUpdated }.Contains(topic))
        {
            ProductDto dto = JsonSerializer.Deserialize<ProductDto>(message,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            if (topic == _settings.Topics.ProductCreated)
                await productsService.AddProduct(dto);
            else if (topic == _settings.Topics.ProductUpdated)
                await productsService.UpdateProductInfo(dto);
        }
        else if (topic == _settings.Topics.ProductDeleted)
            await productsService.DeleteProductById(int.Parse(message));
    }
}
