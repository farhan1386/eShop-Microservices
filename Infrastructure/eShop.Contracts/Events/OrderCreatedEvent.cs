using eShop.Contracts.Common;
using eShop.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Contracts.Events
{
    public class OrderCreatedEvent : IntegrationEvent
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
