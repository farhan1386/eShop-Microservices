using eShop.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace eShop.NotificationService.Worker.Consumers;

public class CustomerRegisteredConsumer : IConsumer<CustomerRegisteredEvent>
{
    private readonly ILogger<CustomerRegisteredConsumer> _logger;

    public CustomerRegisteredConsumer(ILogger<CustomerRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CustomerRegisteredEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Processing welcome email for {FirstName} {LastName} ({Email}) [CustomerId: {CustomerId}, CorrelationId: {CorrelationId}]",
            @event.FirstName, @event.LastName, @event.Email, @event.CustomerId, @event.CorrelationId);

        await Task.Delay(200, context.CancellationToken);

        _logger.LogInformation("Welcome email sent to {Email}", @event.Email);
    }
}
