using eShop.Contracts.Common;
using eShop.EventBus;
using System;

namespace eShop.Contracts.Events
{
    public class OrderPaidEvent : IntegrationEvent
    {
        public Guid OrderUid { get; set; }
    }
}