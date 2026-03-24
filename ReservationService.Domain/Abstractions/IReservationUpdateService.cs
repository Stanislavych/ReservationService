using ReservationService.Domain.Reservations;

namespace ReservationService.Domain.Abstractions
{
    public interface IReservationUpdateService
    {
        Task<Reservation> ConfirmAsync(int reservationId, long version, CancellationToken cancellationToken);
        Task<Reservation> CancelAsync(int reservationId, long version, CancellationToken cancellationToken);
        Task<Reservation> CompleteAsync(int reservationId, long version, CancellationToken cancellationToken);
    }
}
