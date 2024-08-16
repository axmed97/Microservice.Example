using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        readonly OrderDbContext _dbContext;

        public PaymentFailedEventConsumer(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == context.Message.OrderId);
            order.OrderStatus = Enums.OrderStatus.Failed;
            await _dbContext.SaveChangesAsync();
        }
    }
}
