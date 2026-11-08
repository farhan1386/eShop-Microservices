using eShop.Contracts.Events;
using eShop.OrderService.BusinessLogic.Interfaces;
using eShop.OrderService.Models;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace eShop.OrderService.BusinessLogic.Services
{
    public class OutboxPublisherWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxPublisherWorker> _logger;
        private const int BatchSize = 20;
        private const int PollingDelayMilliseconds = 5000;

        public OutboxPublisherWorker(IServiceProvider serviceProvider, ILogger<OutboxPublisherWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Transactional Outbox Publisher Background Service started running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxEventsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled critical error occurred within the Outbox Publisher loop.");
                }

                await Task.Delay(PollingDelayMilliseconds, stoppingToken);
            }
        }

        private async Task ProcessOutboxEventsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var pendingEvents = await outboxRepo.GetPending(BatchSize);

            foreach (var @event in pendingEvents)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    if (@event.f_event_type == nameof(OrderSubmittedEvent))
                    {
                        var payload = JsonSerializer.Deserialize<OrderSubmittedEvent>(@event.f_payload);
                        if (payload is not null) await publishEndpoint.Publish(payload, stoppingToken);
                    }
                    else if (@event.f_event_type == nameof(OrderPaidEvent))
                    {
                        var payload = JsonSerializer.Deserialize<OrderPaidEvent>(@event.f_payload);
                        if (payload is not null) await publishEndpoint.Publish(payload, stoppingToken);
                    }
                    else
                    {
                        throw new NotSupportedException($"Event type framework context '{@event.f_event_type}' is not supported.");
                    }

                    await outboxRepo.UpdateStatus(@event.f_uid, "Processed", DateTime.UtcNow, null);
                    _logger.LogInformation("Successfully published outbox event: {EventId} of type {EventType}", @event.f_uid, @event.f_event_type);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish outbox transaction row: {EventId}", @event.f_uid);
                    await outboxRepo.UpdateStatus(@event.f_uid, "Failed", null, ex.ToString());
                }
            }
        }
    }
}