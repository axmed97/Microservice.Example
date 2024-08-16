using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Shared.Events;

namespace Order.API.Consumers
{
    public class StockNotReservedEventConsumer : IConsumer<StockNotReservedEvent>
    {
        readonly OrderDbContext _dbContext;

        public StockNotReservedEventConsumer(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == context.Message.OrderId);
            order.OrderStatus = Enums.OrderStatus.Failed;
            await _dbContext.SaveChangesAsync();
        }
    }
}
