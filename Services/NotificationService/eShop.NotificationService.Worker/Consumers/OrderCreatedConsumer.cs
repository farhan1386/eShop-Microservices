using eShop.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace eShop.NotificationService.Worker.Consumers
{
    public class OrderSubmittedConsumer : IConsumer<OrderSubmittedEvent>
    {
        private readonly ILogger<OrderSubmittedConsumer> _logger;

        public OrderSubmittedConsumer(ILogger<OrderSubmittedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderSubmittedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing order confirmation outbox event for Order UID: {OrderUid} (Customer UID: {CustomerUid})",
                @event.OrderUid, @event.CustomerUid);

            await Task.Delay(200, context.CancellationToken);

            _logger.LogInformation("Order confirmation alert successfully sent for Order {OrderUid}", @event.OrderUid);
        }
    }
}