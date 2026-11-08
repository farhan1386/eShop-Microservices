using eShop.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Contracts.Events
{
    public class InventoryReservedEvent : IntegrationEvent
    {
        public Guid OrderId { get; set; }
        public List<ReservedItemDto> Items { get; set; }
    }

    public class ReservedItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
