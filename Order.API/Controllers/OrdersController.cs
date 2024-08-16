using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Data;
using Order.API.Models;
using Order.API.ViewModels;
using Shared.Events;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(OrderDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderVM createOrder)
        {
            Models.Order order = new()
            {
                OrderId = Guid.NewGuid(),
                BuyerId = createOrder.BuyerId,
                CreatedDate = DateTime.Now,
                OrderStatus = Enums.OrderStatus.Suspend,
                TotalPrice = createOrder.CreateOrderItemVMs.Sum(x => x.Count * x.Price) ,
            };

            order.OrderItems = createOrder.CreateOrderItemVMs.Select(x => new OrderItem
            {
                Id = Guid.NewGuid(),
                Count = x.Count,
                Price = x.Price,
                ProductId = x.ProductId,
                OrderId = order.OrderId,
            }).ToList();

            await _context.AddAsync(order);
            await _context.SaveChangesAsync();

            OrderCreatedEvent orderCreatedEvent = new()
            {
                OrderId = order.OrderId,
                BuyerId = createOrder.BuyerId,
                TotalPrice = order.TotalPrice,
                OrderItems = order.OrderItems.Select(x => new Shared.Messages.OrderItemMessage
                {
                    Count = x.Count,
                    ProductId = x.ProductId,
                }).ToList()
            };

            await _publishEndpoint.Publish<OrderCreatedEvent>(orderCreatedEvent);
            return Ok();
        }
    }
}
