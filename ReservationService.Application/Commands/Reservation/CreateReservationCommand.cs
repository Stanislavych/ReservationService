using MediatR;
using Microsoft.AspNetCore.Http;
using ReservationService.Application.Base;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;
using ReservationService.Domain.Reservations.ValueObjects;
using System.Text.Json;

namespace ReservationService.Application.Commands.Reservation
{
    public record CreateReservationCommand(string Name, int GuestsCount, DateTime StartTime,
        DateTime EndTime, string Wish, int TableId, int UserId) : IdempotentCommand<ReservationDto>
    {
        public class Handler : IRequestHandler<CreateReservationCommand, ReservationDto>
        {
            private readonly IReservationCreationService _reservationCreationService;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IOutboxRepository _outboxRepository;
            private readonly IReservationRepository _reservationRepository;

            public Handler(IUnitOfWork unitOfWork, IOutboxRepository outboxRepository, IReservationRepository reservationRepository, IReservationCreationService reservationCreationService)
            {
                _unitOfWork = unitOfWork;
                _outboxRepository = outboxRepository;
                _reservationRepository = reservationRepository;
                _reservationCreationService = reservationCreationService;
            }
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

               await _reservationRepository.AddAsync(reservation, cancellationToken);

                foreach (var @event in reservation.DomainEvents)
                {
                    var outboxMessage = new OutboxMessage(
                        @event.GetType().Name,
                        JsonSerializer.Serialize(@event, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));

                    await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

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
