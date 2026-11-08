using eShop.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace eShop.NotificationService.Worker.Consumers;

public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(ILogger<PaymentCompletedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Processing payment receipt for Payment {PaymentId} (Order: {OrderId}, Amount: {Amount:C}, Status: {Status}, CorrelationId: {CorrelationId})",
            @event.PaymentId, @event.OrderId, @event.Amount, @event.Status, @event.CorrelationId);

        await Task.Delay(200, context.CancellationToken);

        _logger.LogInformation("Payment receipt sent for Order {OrderId}", @event.OrderId);
    }
}