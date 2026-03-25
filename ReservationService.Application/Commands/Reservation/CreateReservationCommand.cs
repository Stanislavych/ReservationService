using MediatR;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;
using ReservationService.Domain.Reservations.ValueObjects;
using System.Text.Json;

namespace ReservationService.Application.Commands.Reservation
{
    public record CreateReservationCommand(string Name, int GuestsCount, DateTime StartTime,
        DateTime EndTime, string Wish, int TableId, int UserId) : IRequest<ReservationDto>
    {
        public class Handler(IReservationRepository reservationRepository, IUnitOfWork unitOfWork,
            IReservationCreationService _reservationCreationService, IOutboxRepository outboxRepository)
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
