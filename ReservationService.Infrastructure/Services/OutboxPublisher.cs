using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using ReservationService.Domain.Abstractions;
using System.Text.Json;
using MassTransit;

namespace ReservationService.Infrastructure.Services
{
    public class OutboxPublisher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisher> _logger;
        private readonly int _batchSize = 50;
        private readonly int _intervalSeconds = 5;

        public OutboxPublisher(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Publisher started");

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_intervalSeconds));

            while(!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessOutboxMessageAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox messages");
                }
            }

            _logger.LogInformation("Outbox Publisher stopped");
        }

        private async Task ProcessOutboxMessageAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var messages = await outboxRepository.GetUnpublishedMessagesAsync(_batchSize, cancellationToken);

            if (!messages.Any())
                return;

            _logger.LogInformation("Found {Count} unpublished messages", messages.Count());

            foreach (var message in messages)
            {
                try
                {
                    var eventType = Type.GetType($"ReservationService.Domain.Events.{message.EventType}");
                    var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType);

                    await publishEndpoint.Publish(domainEvent, cancellationToken);

                    message.MarkAsPublished();
                    
                    await outboxRepository.UpdateAsync(message,cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish message {MessageId}", message.Id);

                    message.MarkAsFailed(ex.Message);
                    
                    await outboxRepository.UpdateAsync(message, cancellationToken);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Processed {Count} messages", messages.Count());
        }
    }
}
