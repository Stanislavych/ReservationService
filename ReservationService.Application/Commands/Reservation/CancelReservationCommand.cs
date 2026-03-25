using MediatR;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;
using System.Text.Json;

namespace ReservationService.Application.Commands.Reservation
{
    public record CancelReservationCommand(int Id, long Version) : IRequest<ReservationDto>
    {
        public class Handler(IReservationUpdateService reservationUpdateService, IUnitOfWork unitOfWork, IOutboxRepository outboxRepository)
            : IRequestHandler<CancelReservationCommand, ReservationDto>
        {
            public async Task<ReservationDto> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
            {
                var reservation = await reservationUpdateService.CancelAsync(request.Id, request.Version, cancellationToken);

                foreach (var @event in reservation.DomainEvents)
                {
                    var outboxMessage = new OutboxMessage(
                        @event.GetType().Name,
                        JsonSerializer.Serialize(@event, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));

                    await outboxRepository.AddAsync(outboxMessage, cancellationToken);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);

                reservation.ClearDomainEvents();

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
