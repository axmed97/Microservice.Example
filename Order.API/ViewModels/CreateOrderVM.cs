﻿namespace Order.API.ViewModels
{
    public class CreateOrderVM
    {
        public Guid BuyerId { get; set; }
        public ICollection<CreateOrderItemVM> CreateOrderItemVMs { get; set; }
    }
}
