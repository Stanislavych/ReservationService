using MediatR;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;

namespace ReservationService.Application.Commands.Reservation
{
    public record ConfirmReservationCommand(int Id, long Version) : IRequest<ReservationDto>
    {
        public class Handler(IReservationUpdateService reservationUpdateService, IUnitOfWork unitOfWork) 
            : IRequestHandler<ConfirmReservationCommand, ReservationDto>
        {
            public async Task<ReservationDto> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
            {
                var reservation = await reservationUpdateService.ConfirmAsync(request.Id, request.Version, cancellationToken);

                await unitOfWork.SaveChangesAsync();

                return new ReservationDto(
                    reservation.Id,
                    reservation.Name,
                    reservation.GuestsCount.Value,
                    reservation.ReservationTime.Start,
                    reservation.ReservationTime.End,
                    reservation.Wish,
                    reservation.Status.ToString(),
                    reservation.TableId,
                    reservation.UserId,
                    reservation.Version
                    );
            }
        }
    }
}
