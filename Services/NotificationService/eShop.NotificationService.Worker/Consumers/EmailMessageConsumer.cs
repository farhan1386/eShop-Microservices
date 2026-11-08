using eShop.Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace eShop.NotificationService.Worker.Consumers;

public class EmailMessageConsumer : IConsumer<EmailMessage>
{
    private readonly ILogger<EmailMessageConsumer> _logger;

    public EmailMessageConsumer(ILogger<EmailMessageConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmailMessage> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Sending email to {To} with subject {Subject} [Uid: {Uid}]",
            message.To, message.Subject, message.f_uid);

        await Task.Delay(200, context.CancellationToken);

        _logger.LogInformation("Email sent to {To}", message.To);
    }
}