using ReservationService.Domain.Reservations;
using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Domain.Abstractions
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        Task<bool> HasConflictingReservationsAsync(int tableId, TimeRange timeRange, CancellationToken cancellationToken = default);
        Task<bool> UpdateWithVersionAsync(Reservation reservation, long expectedVersion, CancellationToken cancellationToken = default);
    }
}
