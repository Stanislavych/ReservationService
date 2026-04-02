using MediatR;
using Microsoft.AspNetCore.Http;
using ReservationService.Application.Base;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;
using System.Security.Claims;

namespace ReservationService.Application.Queries.Reservation
{
    public record GetReservationCommand(int Id) : IRequest<ReservationDto>
    {
        public class Handler : BaseHandler<GetReservationCommand, ReservationDto>
        {
            private readonly IReservationRepository _reservationRepository;

            public Handler(IReservationRepository reservationRepository, IHttpContextAccessor httpContextAccessor) : base (httpContextAccessor)
            {
                _reservationRepository = reservationRepository;
            }

            public override async Task<ReservationDto> Handle(GetReservationCommand request, CancellationToken cancellationToken)
            {
                var currentReservation = await _reservationRepository.GetByIdAsync(request.Id, cancellationToken);

                if (currentReservation == null)
                    throw new ArgumentException($"Reservation with id {request.Id} not found");

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                if (currentUserRole == "Customer" && currentReservation.UserId != currentUserId)
                    throw new UnauthorizedAccessException("You don't have permisson to view this reservation");

                return new ReservationDto(
                    currentReservation.Id,
                    currentReservation.Name,
                    currentReservation.GuestsCount.Value,
                    currentReservation.ReservationTime.Start,
                    currentReservation.ReservationTime.End,
                    currentReservation.Wish,
                    currentReservation.Status.ToString(),
                    currentReservation.TableId,
                    currentReservation.UserId,
                    currentReservation.Version
                );
            }
        }
    }
}
