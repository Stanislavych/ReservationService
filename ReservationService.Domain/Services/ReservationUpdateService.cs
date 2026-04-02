using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Exceptions;
using ReservationService.Domain.Reservations;
using System.Data;

namespace ReservationService.Domain.Services
{
    public class ReservationUpdateService : IReservationUpdateService
    {
        private readonly IReservationRepository _reservationRepository;

        public ReservationUpdateService(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public Task<Reservation> CancelAsync(int reservationId, long version, int currentUserId, string currentUserRole, CancellationToken cancellationToken)
        => ExecuteUpdateAsync(reservationId, version, currentUserId, currentUserRole, r => r.Cancel(), cancellationToken);

        public Task<Reservation> CompleteAsync(int reservationId, long version, int currentUserId, string currentUserRole, CancellationToken cancellationToken)
        => ExecuteUpdateAsync(reservationId, version, currentUserId, currentUserRole, r => r.Complete(), cancellationToken);

        public Task<Reservation> ConfirmAsync(int reservationId, long version, int currentUserId, string currentUserRole, CancellationToken cancellationToken)
        => ExecuteUpdateAsync(reservationId, version, currentUserId, currentUserRole, r => r.Confirm(), cancellationToken);

        private async Task<Reservation> ExecuteUpdateAsync(
            int reservationId,
            long version,
            int currentUserId,
            string currentUserRole,
            Action<Reservation> operation,
            CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);

            if (reservation == null)
                throw new NotFoundException($"Reservation {reservationId} not found");
            if (currentUserRole == "Customer" && reservation.UserId != currentUserId)
                throw new UnauthorizedAccessException("You don't have permission to modify this reservation");
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
