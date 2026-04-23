using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Exceptions;
using ReservationService.Domain.Reservations;
using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Domain.Services
{
    public class ReservationCreationService : IReservationCreationService
    {
        private readonly ITableRepository _tableRepository;
        private readonly IReservationRepository _reservationRepository;

        public ReservationCreationService(ITableRepository tableRepository, IReservationRepository reservationRepository)
        {
            _tableRepository = tableRepository;
            _reservationRepository = reservationRepository;
        }

        public async Task<Reservation> CreateAsync(
            string name,
            GuestsCount guestsCount,
            TimeRange timeRange,
            string wish,
            int tableId,
            int userId,
            CancellationToken cancellationToken)
        {
            var table = await _tableRepository.GetByIdAsync(tableId, cancellationToken);
            if (table == null)
                throw new NotFoundException($"Table with id {tableId} not found");

            if (table.Capacity.Value < guestsCount.Value)
                throw new DomainException($"Capacity exceeded");

            var hasConflicts = await _reservationRepository.HasConflictingReservationsAsync(
                tableId, timeRange, cancellationToken);

            if (hasConflicts)
                throw new DomainException($"Table is not available at the requested time");

            return new Reservation(name, guestsCount, timeRange, wish, tableId, userId, DateTime.UtcNow);
        }
    }
}
