using Confluent.Kafka;
using Microsoft.Extensions.Options;
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
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_settings.Topics.ManageProduct);
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
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки запроса");
            }
        }
        _consumer.Close();
    }

    public async Task ProccessRequestAsync(IProductsService productsService,string topic,string message)
    {
        if (topic==_settings.Topics.ManageProduct)
        {
            KafkaEvent<ProductDto>? dto = JsonSerializer.Deserialize<KafkaEvent<ProductDto>>(message,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto == null)
                throw new ArgumentException("Пустой json");
            _logger.LogInformation($"Обработка ивента типа {dto.EventType}");
            if (dto.EventType == "product_added" && dto.Data != null)
                await productsService.AddProduct(dto.Data);
            else if (dto.EventType == "product_updated" && dto.Data != null)
                await productsService.UpdateProductInfo(dto.Data);
            else if (dto.EventType == "product_deleted")
                await productsService.DeleteProductById(dto.Id);
            else
                throw new ArgumentException("Неверный тип ивента, есть только: product_added, product_updated, product_deleted");
        }
    }
}
