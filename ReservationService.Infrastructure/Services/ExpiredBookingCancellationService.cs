using MassTransit.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReservationService.Application.Interfaces;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;
using ReservationService.Infrastructure.Data.Configurations;
using System.ComponentModel;
using System.Text.Json;

namespace ReservationService.Infrastructure.Services
{
    public class ExpiredBookingCancellationService : IExpiredBookingCancellationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IReservationUpdateService _reservationUpdateService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOutboxRepository _outboxRepository;
        private readonly ILogger<ExpiredBookingCancellationService> _logger;
        private readonly TimeSpan _pendingPaymentTimeout;

        public ExpiredBookingCancellationService(
            IReservationRepository reservationRepository,
            IReservationUpdateService reservationUpdateService,
            IUnitOfWork unitOfWork, IOutboxRepository outboxRepository,
            ILogger<ExpiredBookingCancellationService> logger,
            IOptions<ExpiredBookingSettings> settings)
        {
            _reservationRepository = reservationRepository;
            _reservationUpdateService = reservationUpdateService;
            _unitOfWork = unitOfWork;
            _outboxRepository = outboxRepository;
            _logger = logger;
            _pendingPaymentTimeout = TimeSpan.FromMinutes(settings.Value.PendingPaymentTimeoutMinutes);
        }

        public async Task CancelExpiredBookingsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Checking for expired pending payment bookings...");

            var cutoffTime = DateTime.UtcNow.Subtract(_pendingPaymentTimeout);
            var expiredBookings = await _reservationRepository.GetExpiredPendingPaymentsAsync(cutoffTime, cancellationToken);

            if (!expiredBookings.Any())
            {
                _logger.LogInformation("No expired bookings found");

                return;
            }

            _logger.LogInformation("Found {Count} expired bookings to cancel", expiredBookings.Count());

            foreach (var booking in expiredBookings)
            {
                try
                {
                    const int systemUserId = 0;
                    const string systemUserRole = "System";

                    var cancelledBooking = await _reservationUpdateService.CancelAsync(
                        booking,
                        booking.Version,
                        systemUserId,
                        systemUserRole,
                        cancellationToken);

                    foreach (var @event in cancelledBooking.DomainEvents)
                    {
                        var outboxMessage = new OutboxMessage(
                            @event.GetType().Name,
                            JsonSerializer.Serialize(@event, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }));

                        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    cancelledBooking.ClearDomainEvents();

                    _logger.LogInformation("Successfully cancelled expired booking {BookingId}", booking.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cancel expired booking {BookingId}", booking.Id);
                }

                _logger.LogInformation("Finished processing expired bookings.");
            }
        }
    }
}
