using MediatR;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;

namespace ReservationService.Application.Queries.Reservation
{
    public record GetReservationCommand(int Id) : IRequest<ReservationDto>
    {
        public class Handler(IReservationRepository reservationRepository) : IRequestHandler<GetReservationCommand, ReservationDto>
        {
            public async Task<ReservationDto> Handle(GetReservationCommand request, CancellationToken cancellationToken)
            {
                var currentReservation = await reservationRepository.GetByIdAsync(request.Id, cancellationToken);

                if (currentReservation == null)
                    throw new ArgumentException($"Reservation with id {request.Id} not found");

                //добавить логику Customer - свою, Admin - любую

                return new ReservationDto(
                    currentReservation.Id,
                    currentReservation.Name,
                    currentReservation.GuestsCount.Value,
                    currentReservation.ReservationTime.Start,
                    currentReservation.ReservationTime.End,
                    currentReservation.Wish,
                    currentReservation.Status.ToString(),
                    currentReservation.TableId, // можно расширить представление до TableInfoDto
                    currentReservation.UserId   // можно расширить представление до UserInfoDto
                );
            }
        }
    }
}
