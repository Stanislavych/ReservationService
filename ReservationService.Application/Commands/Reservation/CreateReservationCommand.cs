using MediatR;
using ReservationService.Application.DTOs;
using ReservationService.Application.Interfaces;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Application.Commands.Reservation
{
    public record CreateReservationCommand(string Name, int GuestsCount, DateTime StartTime,
        DateTime EndTime, string Wish, int TableId, int UserId) : IRequest<ReservationDto>
    {
        public class Handler(IReservationRepository reservationRepository, IUnitOfWork unitOfWork, IReservationCreationService _reservationCreationService)
            : IRequestHandler<CreateReservationCommand, ReservationDto>
        {
            public async Task<ReservationDto> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
            {
                var timeRange = TimeRange.Create(request.StartTime, request.EndTime);
                var guestsCount = Domain.Reservations.ValueObjects.GuestsCount.Create(request.GuestsCount);

                var reservation = await _reservationCreationService.CreateAsync(
                    request.Name,
                    guestsCount,
                    timeRange,
                    request.Wish,
                    request.TableId,
                    request.UserId,
                    cancellationToken
                    );

               await reservationRepository.AddAsync(reservation, cancellationToken);
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
                    reservation.UserId
                    );
            }
        }
    }
}
