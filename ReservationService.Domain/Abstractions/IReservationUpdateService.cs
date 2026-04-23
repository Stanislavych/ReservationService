using ReservationService.Domain.Reservations;

namespace ReservationService.Domain.Abstractions
{
    public interface IReservationUpdateService
    {
        Task<Reservation> ConfirmAsync(Reservation reservation, long version, CancellationToken cancellationToken);
        Task<Reservation> CancelAsync(Reservation reservation, long version, CancellationToken cancellationToken);
        Task<Reservation> CompleteAsync(Reservation reservation, long version, CancellationToken cancellationToken);
    }
}
