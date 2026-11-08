using eShop.Contracts.Common;
using eShop.EventBus;
using System;

namespace eShop.Contracts.Events
{
    public class OrderSubmittedEvent : IntegrationEvent
    {
        public Guid OrderUid { get; set; }
        public Guid CustomerUid { get; set; }
    }
}