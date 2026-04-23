using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Exceptions;
using ReservationService.Domain.Reservations;

namespace ReservationService.Domain.Services
{
    public class ReservationUpdateService : IReservationUpdateService
    {
        private readonly IReservationRepository _reservationRepository;

        public ReservationUpdateService(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public Task<Reservation> CancelAsync(Reservation reservation, long version, CancellationToken cancellationToken)
        => ExecuteUpdateAsync(reservation, version, r => r.Cancel(), cancellationToken);

        public Task<Reservation> CompleteAsync(Reservation reservation, long version, CancellationToken cancellationToken)
        => ExecuteUpdateAsync(reservation, version, r => r.Complete(), cancellationToken);

        public Task<Reservation> ConfirmAsync(Reservation reservation, long version, CancellationToken cancellationToken)
        => ExecuteUpdateAsync(reservation, version, r => r.Confirm(), cancellationToken);

        private async Task<Reservation> ExecuteUpdateAsync(
            Reservation reservation,
            long version,
            Action<Reservation> operation,
            CancellationToken cancellationToken)
        {
            if (reservation.Version != version)
                throw new ConcurrencyException("Reservation was modified by another user. Please reload and try again.");

            operation(reservation);

            var updated = await _reservationRepository.UpdateWithVersionAsync(reservation, version, cancellationToken);

            if (!updated)
                throw new ConcurrencyException("Reservation was modified by another user. Please reload and try again.");

            return reservation;
        }
    }
}
