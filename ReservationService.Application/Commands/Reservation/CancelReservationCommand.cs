using MediatR;
using Microsoft.AspNetCore.Http;
using ReservationService.Application.Base;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;
using ReservationService.Domain.Exceptions;
using System.Text.Json;

namespace ReservationService.Application.Commands.Reservation
{
    public record CancelReservationCommand(int Id, long Version) : IRequest<ReservationDto>
    {
        public class Handler : BaseHandler<CancelReservationCommand, ReservationDto>
        {
            private readonly IReservationUpdateService _reservationUpdateService;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IOutboxRepository _outboxRepository;
            private readonly IReservationRepository _reservationRepository;

            public Handler(IReservationUpdateService reservationUpdateService, IUnitOfWork unitOfWork,
                IOutboxRepository outboxRepository, IHttpContextAccessor httpContextAccessor, IReservationRepository reservationRepository) : base(httpContextAccessor)
            {
                _reservationUpdateService = reservationUpdateService;
                _unitOfWork = unitOfWork;
                _outboxRepository = outboxRepository;
                _reservationRepository = reservationRepository;
            }

            public override async Task<ReservationDto> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
            {

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();
                var reservation = await _reservationRepository.GetByIdAsync(request.Id, cancellationToken);

                if (reservation == null)
                    throw new NotFoundException($"Reservation {request.Id} not found");

                var cancelledReservation = await _reservationUpdateService.CancelAsync(reservation, request.Version, currentUserId, currentUserRole, cancellationToken);

                foreach (var @event in cancelledReservation.DomainEvents)
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

                cancelledReservation.ClearDomainEvents();

                return new ReservationDto(
                    cancelledReservation.Id,
                    cancelledReservation.Name,
                    cancelledReservation.GuestsCount.Value,
                    cancelledReservation.ReservationTime.Start,
                    cancelledReservation.ReservationTime.End,
                    cancelledReservation.Wish,
                    cancelledReservation.Status.ToString(),
                    cancelledReservation.TableId,
                    cancelledReservation.UserId,
                    cancelledReservation.Version
                    );
            }
        }
    }
}
