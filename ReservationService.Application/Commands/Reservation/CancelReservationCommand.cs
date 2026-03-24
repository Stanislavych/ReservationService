using MediatR;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;

namespace ReservationService.Application.Commands.Reservation
{
    public record CancelReservationCommand(int Id, long Version) : IRequest<ReservationDto>
    {
        public class Handler(IReservationUpdateService reservationUpdateService, IUnitOfWork unitOfWork)
            : IRequestHandler<CancelReservationCommand, ReservationDto>
        {
            public async Task<ReservationDto> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
            {
                var reservation = await reservationUpdateService.CancelAsync(request.Id, request.Version, cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

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
