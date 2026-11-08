using eShop.Contracts.Common;
using eShop.Contracts.Enums;

namespace eShop.Contracts.Events
{
    public class PaymentCompletedEvent : IntegrationEvent
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
