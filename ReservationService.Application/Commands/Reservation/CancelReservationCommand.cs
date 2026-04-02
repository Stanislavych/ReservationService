using MediatR;
using Microsoft.AspNetCore.Http;
using ReservationService.Application.Base;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;
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

            public Handler(IReservationUpdateService reservationUpdateService, IUnitOfWork unitOfWork,
                IOutboxRepository outboxRepository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
            {
                _reservationUpdateService = reservationUpdateService;
                _unitOfWork = unitOfWork;
                _outboxRepository = outboxRepository;
            }

            public override async Task<ReservationDto> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
            {

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();
                var reservation = await _reservationUpdateService.CancelAsync(request.Id, request.Version, currentUserId, currentUserRole, cancellationToken);

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
