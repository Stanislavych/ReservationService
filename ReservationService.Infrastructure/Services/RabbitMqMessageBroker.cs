using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ReservationService.Application.Interfaces;
using ReservationService.Infrastructure.Data.Configurations;
using System.Text;

namespace ReservationService.Infrastructure.Services
{
    public class RabbitMqMessageBroker : IMessageBroker, IAsyncDisposable
    {
        private readonly RabbitMqConnection _connection;
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqMessageBroker> _logger;
        private IChannel _channel;
        private bool _initialized;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        public RabbitMqMessageBroker(RabbitMqConnection connection, IOptions<RabbitMqSettings> settings, ILogger<RabbitMqMessageBroker> logger)
        {
            _connection = connection;
            _settings = settings.Value;
            _logger = logger;
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initialized && _channel?.IsOpen == true) return;

            await _initLock.WaitAsync();

            try
            {
                if (_initialized && _channel?.IsOpen == true) return;

                var conn = await _connection.GetConnectionAsync();

                _channel = await conn.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(
                    exchange: _settings.Exchange.Name,
                    type: _settings.Exchange.Type,
                    durable: _settings.Exchange.Durable);

                foreach (var (eventType, queueName) in _settings.Queues)
                {
                    await _channel.QueueDeclareAsync(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false);

                    await _channel.QueueBindAsync(
                         queue: queueName,
                         exchange: _settings.Exchange.Name,
                         routingKey: eventType);

                    _logger.LogDebug("Created queue {Queue} for event {EventType}", queueName, eventType);
                }

                _initialized = true;
                _logger.LogInformation("RabbitMQ broker initialized");
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task PublishAsync(string eventType, string payload, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync();

            var body = Encoding.UTF8.GetBytes(payload);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Headers = new Dictionary<string, object?>
                {
                    { "event-type", eventType },
                    { "published-at", DateTime.UtcNow.ToString("O") }
                }
            };

            await _channel.BasicPublishAsync(
                 exchange: _settings.Exchange.Name,
                 routingKey: eventType,
                 mandatory: false,
                 basicProperties: properties,
                 body: body,
                 cancellationToken: cancellationToken);

            _logger.LogDebug("Published event {EventType} to exchange {Exchange}",
                eventType, _settings.Exchange.Name);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();

                _channel.Dispose();
                _channel = null;
            }

            _initLock.Dispose();
        }
    }
}
