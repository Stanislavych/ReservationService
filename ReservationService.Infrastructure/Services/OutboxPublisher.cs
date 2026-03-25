using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using ReservationService.Domain.Abstractions;

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

            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessageAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox messages");
                }

                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds),stoppingToken);
            }

            _logger.LogInformation("Outbox Publisher stopped");
        }

        private async Task ProcessOutboxMessageAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

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
                    _logger.LogInformation("OUTBOX MESSAGE: EventType={EventType}, Payload={Payload}, CreatedAt={CreatedAt}",
                        message.EventType, message.Payload, message.CreatedAt);

                    //logic for rabbitMQ

                    message.MarkAsPublished();
                    
                    await outboxRepository.UpdateAsync(message,cancellationToken);

                    _logger.LogDebug("Marked message {MessageId} as published", message.Id);
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
