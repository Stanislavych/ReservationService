using MediatR;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;

namespace ReservationService.Application.Commands.Reservation
{
    public record ConfirmReservationCommand(int Id) : IRequest<ReservationDto>
    {
        public class Handler(IReservationRepository reservationRepository, IUnitOfWork unitOfWork) 
            : IRequestHandler<ConfirmReservationCommand, ReservationDto>
        {
            public async Task<ReservationDto> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
            {
                var currentReservation = await reservationRepository.GetByIdAsync(request.Id,cancellationToken);

                if (currentReservation == null)
                    throw new ArgumentException($"Reservation with id {request.Id} not found");

                currentReservation.Confirm();

                reservationRepository.Update(currentReservation);

                await unitOfWork.SaveChangesAsync();

                return new ReservationDto(
                    currentReservation.Id,
                    currentReservation.Name,
                    currentReservation.GuestsCount.Value,
                    currentReservation.ReservationTime.Start,
                    currentReservation.ReservationTime.End,
                    currentReservation.Wish,
                    currentReservation.Status.ToString(),
                    currentReservation.TableId,
                    currentReservation.UserId
                    );
            }
        }
    }
}
