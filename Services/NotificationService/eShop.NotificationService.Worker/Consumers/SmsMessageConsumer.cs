using eShop.Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace eShop.NotificationService.Worker.Consumers;

public class SmsMessageConsumer : IConsumer<SmsMessage>
{
    private readonly ILogger<SmsMessageConsumer> _logger;

    public SmsMessageConsumer(ILogger<SmsMessageConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SmsMessage> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Sending SMS to {PhoneNumber}: {MessageText} [Uid: {Uid}]",
            message.PhoneNumber, message.MessageText, message.f_uid);

        await Task.Delay(200, context.CancellationToken);

        _logger.LogInformation("SMS sent to {PhoneNumber}", message.PhoneNumber);
    }
}