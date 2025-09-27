using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using VenomPizzaMenuService.src.cache;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.kafka;
using VenomPizzaMenuService.src.repository;
using VenomPizzaMenuService.src.service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"));
    return ConnectionMultiplexer.Connect(config);
});
builder.Services.AddScoped<ProductsService>();
builder.Services.AddScoped<ProductsRepository>();
builder.Services.AddScoped<ICacheProvider, CacheProvider>();

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton(provider =>
{
    var config = new ProducerConfig { BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] };
    return new ProducerBuilder<string,string>(config).Build();
});
builder.Services.AddSingleton(provider =>
{
    var kafkaSettings = provider.GetRequiredService<IOptions<KafkaSettings>>().Value;
    var config = new ConsumerConfig
    {
        BootstrapServers = kafkaSettings.BootstrapServers,
        GroupId = kafkaSettings.GroupId,
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,
    };
    return new ConsumerBuilder<string,string>(config).Build();
});
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyCorsPolicy",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();
builder.Logging.AddFilter((provider, category, logLevel) =>
{
    if (category.Contains("Microsoft.EntityFrameworkCore"))
    {
        return false;
    }
    return true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<ProductsDbContext>();
    await context.Database.MigrateAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
}
app.UseCors("MyCorsPolicy");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();