using ReservationService.Application.Commands.Reservation;

namespace ReservationService.Application.Interfaces
{
    public interface IReservationValidationService
    {
        Task ValidateAsync(CreateReservationCommand command, CancellationToken cancellationToken = default);
    }
}
