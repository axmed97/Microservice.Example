using MassTransit;
using Microsoft.OpenApi.Validations;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using StockAPI.Services;

namespace StockAPI.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        IMongoCollection<Models.Stock> _mongoCollection;
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IPublishEndpoint _publishEndpoint;
        public OrderCreatedEventConsumer(MongoDbService dbService, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _mongoCollection = dbService.GetCollection<Models.Stock>();
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();
            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add((await _mongoCollection.FindAsync(x => x.ProductId == item.ProductId && x.Count >= item.Count)).Any());
            }

            if (stockResult.TrueForAll(x => x.Equals(true)))
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await (await _mongoCollection.FindAsync(x => x.ProductId == item.ProductId)).FirstOrDefaultAsync();
                    stock.Count -= item.Count;
                    await _mongoCollection.FindOneAndReplaceAsync(x => x.ProductId == item.ProductId, stock);
                }

                StockReservedEvent stockReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    TotalPrice = context.Message.TotalPrice,
                };
                ISendEndpoint sendEndpoint = 
                    await _sendEndpointProvider.GetSendEndpoint
                    (new Uri($"queue: {RabbitMQSettings.Payment_StockReservedEventQueue}"));
                await sendEndpoint.Send(stockReservedEvent);
            }
            else{
                StockNotReservedEvent stockNotReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    Message = "Failed"
                };
                
                await _publishEndpoint.Publish(stockNotReservedEvent);
            }
        }
    }
}
