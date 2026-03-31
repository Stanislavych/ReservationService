using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReservationService.Infrastructure.Data.Configurations;

namespace ReservationService.Infrastructure.Services
{
    public class RabbitMqConnection : IAsyncDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqConnection> _logger;
        private IConnection _connection;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private bool _disposed;

        public RabbitMqConnection(IOptions<RabbitMqSettings> options, ILogger<RabbitMqConnection> logger)
        {
            _settings = options.Value;
            _logger = logger;

            _ = EnsureConnectedAsync();
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            await EnsureConnectedAsync();

            return _connection;
        }

        private async Task EnsureConnectedAsync()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqConnection));
            if (_connection?.IsOpen == true) return;

            await _lock.WaitAsync();

            try
            {
                if (_connection?.IsOpen == true) return;

                _logger.LogInformation("Connecting to RabbitMQ at {Host}:{Port}", _settings.Host, _settings.Port);

                var factory = new ConnectionFactory
                {
                    HostName = _settings.Host,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                    RequestedHeartbeat = TimeSpan.FromSeconds(60),
                };

                _connection = await factory.CreateConnectionAsync();
                _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;

                _logger.LogInformation("Connected to RabbitMQ successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection sutdown: {Reason}", e.ReplyText);
            _connection = null;

            await Task.Delay(TimeSpan.FromSeconds(5));
            await EnsureConnectedAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            if (_connection != null)
            {
                await _connection.CloseAsync();

                _connection.Dispose();
                _connection = null;
            }

            _lock.Dispose();
            _disposed = true;
        }
    }
}
