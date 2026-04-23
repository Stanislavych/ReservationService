using ReservationService.Domain.Reservations;
using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Domain.Abstractions
{
    public interface IReservationCreationService
    {
        Task<Reservation> CreateAsync(
            string name,
            GuestsCount guestsCount,
            TimeRange timeRange,
            string wish,
            int tableId,
            int userId,
            CancellationToken cancellationToken
            );
    }
}
