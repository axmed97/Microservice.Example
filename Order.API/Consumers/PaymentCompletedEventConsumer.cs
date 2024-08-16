using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        readonly OrderDbContext _dbContext;

        public PaymentCompletedEventConsumer(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == context.Message.OrderId);
            order.OrderStatus = Enums.OrderStatus.Completed;
            await _dbContext.SaveChangesAsync();

        }
    }
}
