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
        public class Handler(IReservationRepository reservationRepository, IReservationValidationService validationService, IUnitOfWork unitOfWork)
            : IRequestHandler<CreateReservationCommand, ReservationDto>
        {
            public async Task<ReservationDto> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
            {
                await validationService.ValidateAsync(request, cancellationToken);

                var timeRange = TimeRange.Create(request.StartTime, request.EndTime);
                var guestsCount = Domain.Reservations.ValueObjects.GuestsCount.Create(request.GuestsCount);

                var reservation = Domain.Reservations.Reservation.Create(
                    request.Name,
                    guestsCount,
                    timeRange,
                    request.Wish,
                    request.TableId,
                    request.UserId
                    );

               var createdReservation = await reservationRepository.AddAsync(reservation, cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return new ReservationDto(
                    createdReservation.Id,
                    createdReservation.Name,
                    createdReservation.GuestsCount.Value,
                    createdReservation.ReservationTime.Start,
                    createdReservation.ReservationTime.End,
                    createdReservation.Wish,
                    createdReservation.Status.ToString(),
                    createdReservation.TableId,
                    createdReservation.UserId
                    );
            }
        }
    }
}
