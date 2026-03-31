using MediatR;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Events;
using ReservationService.Infrastructure.Data.Configurations;
using System.Text;
using System.Text.Json;

namespace ReservationService.Infrastructure.Services
{
    public class ReservationEventConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<ReservationEventConsumer> _logger;
        private IConnection _connection;
        private IChannel _channel;

        public ReservationEventConsumer(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqSettings> settings,
            ILogger<ReservationEventConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReservationEventConsumer started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConnectAndConsumeAsync(stoppingToken);
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Consumer error, reconnecting in 5 seconds");
                    
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task ConnectAndConsumeAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.ExchangeDeclareAsync(
                exchange: _settings.Exchange.Name,
                type: _settings.Exchange.Type,
                durable: _settings.Exchange.Durable,
                cancellationToken: cancellationToken
                );

            foreach (var (eventType, queueName) in _settings.Queues)
            {
                await _channel.QueueDeclareAsync(
                     queue: queueName,
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     cancellationToken: cancellationToken);

                await _channel.QueueBindAsync(
                    queue: queueName,
                    exchange: _settings.Exchange.Name,
                    routingKey: eventType,
                    cancellationToken: cancellationToken);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (sender, args) =>
                {
                    await ProcessMessageAsync(args, cancellationToken);
                };

                await _channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken);
            }

            _logger.LogInformation("Consumer connected and listening");
        }

        private async Task ProcessMessageAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
        {
            var body = Encoding.UTF8.GetString(args.Body.ToArray());
            var eventType = args.RoutingKey;

            _logger.LogInformation("Received event {EventType}: {Body}", eventType, body);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                IDomainEvent? domainEvent = eventType switch
                {
                    nameof(ReservationCreated) => JsonSerializer.Deserialize<ReservationCreated>(body),
                    nameof(ReservationConfirmed) => JsonSerializer.Deserialize<ReservationConfirmed>(body),
                    nameof(ReservationCancelled) => JsonSerializer.Deserialize<ReservationCancelled>(body),
                    nameof(ReservationCompleted) => JsonSerializer.Deserialize<ReservationCompleted>(body),
                    _ => throw new NotSupportedException($"Unknown event type: {eventType}")
                };

                await mediator.Publish(domainEvent, cancellationToken);

                await _channel.BasicAckAsync(args.DeliveryTag, false, cancellationToken);
                _logger.LogInformation("Event {EventType} processed successfully", eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event {EventType}", eventType);

                await _channel.BasicNackAsync(args.DeliveryTag, false, false, cancellationToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ReservationEventConsumer stopping...");

            if (_channel != null && _channel.IsOpen)
            {
                await _channel.CloseAsync(cancellationToken);
                await _channel.DisposeAsync();
            }

            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync(cancellationToken);
                _connection.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
