using eShop.Contracts.Common;
using eShop.EventBus;
using System;

namespace eShop.Contracts.Events
{
    public class ShippingStatusUpdatedEvent : IntegrationEvent
    {
        public Guid ShippingManifestId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Comment { get; set; }
    }
}