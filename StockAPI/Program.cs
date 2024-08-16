using MassTransit;
using MongoDB.Driver;
using Shared;
using StockAPI.Consumers;
using StockAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("amqps://fkwmludd:CW1PU7v3m_il5wTInG-ttMN6UvTEJZ8G@hawk.rmq.cloudamqp.com/fkwmludd");
        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(ctx);
        });
    });
});

builder.Services.AddSingleton<MongoDbService>();

#region MongoDb Data Seed
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDbService mongoDbService = scope.ServiceProvider.GetService<MongoDbService>();
var collection = mongoDbService.GetCollection<StockAPI.Models.Stock>();

if (!collection.FindSync(s => true).Any())
{
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 2000 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 3000 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 4000 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 1000 });
    await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 5000 });
}
#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
