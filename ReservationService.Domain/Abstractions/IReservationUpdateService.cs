using ReservationService.Domain.Reservations;

namespace ReservationService.Domain.Abstractions
{
    public interface IReservationUpdateService
    {
        Task<Reservation> ConfirmAsync(Reservation reservation, long version, int currentUserId, string currentUserRole, CancellationToken cancellationToken);
        Task<Reservation> CancelAsync(Reservation reservation, long version,int currentUserId, string currentUserRole, CancellationToken cancellationToken);
        Task<Reservation> CompleteAsync(Reservation reservation, long version, int currentUserId, string currentUserRole, CancellationToken cancellationToken);
    }
}
