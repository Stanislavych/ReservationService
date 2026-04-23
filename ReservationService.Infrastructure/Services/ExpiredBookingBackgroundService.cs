using MassTransit.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReservationService.Application.Interfaces;
using ReservationService.Infrastructure.Data.Configurations;

namespace ReservationService.Infrastructure.Services
{
    public class ExpiredBookingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredBookingBackgroundService> _logger;
        private readonly int _checkIntervalSeconds;

        public ExpiredBookingBackgroundService(IServiceProvider serviceProvider, ILogger<ExpiredBookingBackgroundService> logger,
            IOptions<ExpiredBookingSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _checkIntervalSeconds = settings.Value.CheckIntervalSeconds;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expired Booking Background Service started.");

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_checkIntervalSeconds));

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var cancellationService = scope.ServiceProvider.GetRequiredService<IExpiredBookingCancellationService>();

                    await cancellationService.CancelExpiredBookingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occured while checking expired bookings");
                }
            }

            _logger.LogInformation("Expired Booking Service stopped.");
        }
    }
}
