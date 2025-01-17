using MassTransit;
using PaymentAPI.Consumers;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<StockReservedEventConsumer>();
    configurator.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("amqps://fkwmludd:CW1PU7v3m_il5wTInG-ttMN6UvTEJZ8G@hawk.rmq.cloudamqp.com/fkwmludd");
        cfg.ReceiveEndpoint(RabbitMQSettings.Payment_StockReservedEventQueue, e =>
        {
            e.ConfigureConsumer<StockReservedEventConsumer>(ctx);
        });
    });
});

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
