using eShop.Contracts.Common;
using eShop.EventBus;
using System;

namespace eShop.Contracts.Events
{
    public class ShippingInitiatedEvent : IntegrationEvent
    {
        public Guid ShippingManifestId { get; set; }
        public Guid OrderUid { get; set; }
        public string CarrierName { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}