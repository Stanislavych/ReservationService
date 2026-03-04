using ReservationService.Application.Commands.Reservation;
using ReservationService.Application.Common;
using ReservationService.Application.Interfaces;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Infrastructure.Services
{
    public class ReservationValidationService : IReservationValidationService
    {
        private readonly ITableRepository _tableRepository;
        private readonly IReservationRepository _reservationRepository;

        public ReservationValidationService(ITableRepository tableRepository, IReservationRepository reservationRepository)
        {
            _tableRepository = tableRepository;
            _reservationRepository = reservationRepository;
        }

        public async Task ValidateAsync(CreateReservationCommand command, CancellationToken cancellationToken)
        {
            var table = await _tableRepository.GetByIdAsync(command.TableId, cancellationToken);
            if (table == null)
                throw new ValidationException($"Table with id {command.TableId} not found");

            if (table.Capacity.Value < command.GuestsCount)
                throw new ValidationException($"Table capacity ({table.Capacity.Value}) is less than requested guests ({command.GuestsCount})");

            if (command.StartTime <= DateTime.UtcNow)
                throw new ValidationException($"Start time must be in the future");
            if (command.EndTime <= command.StartTime)
                throw new ValidationException($"End time must be after start time");

            var timeRange = TimeRange.Create(command.StartTime, command.EndTime);
            var hasConflicts = await _reservationRepository.HasConflictingReservationsAsync(
                command.TableId,
                timeRange,
                cancellationToken
                );

            if(hasConflicts)
                throw new ValidationException($"Table is not available at the requested time");
        }
    }
}
