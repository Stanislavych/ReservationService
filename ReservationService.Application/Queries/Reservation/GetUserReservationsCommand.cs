using MediatR;
using Microsoft.AspNetCore.Http;
using ReservationService.Application.Base;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Users;

namespace ReservationService.Application.Queries.Reservation
{
    public record GetUserReservationsCommand(int UserId) : IRequest<List<ReservationDto>>
    {
        public class Handler : BaseHandler<GetUserReservationsCommand, List<ReservationDto>>
        {
            private readonly IReservationRepository _reservationRepository;
            private readonly IRepository<User> _userRepository;

            public Handler(IReservationRepository reservationRepository, IRepository<User> userRepository, IHttpContextAccessor httpContextAccessor)
                : base (httpContextAccessor)
            {
                _reservationRepository = reservationRepository;
                _userRepository = userRepository;
            }

            public override async Task<List<ReservationDto>> Handle(GetUserReservationsCommand request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

                if (user == null)
                    throw new ArgumentException($"Reservations for user with id {request.UserId} not found!");

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                if (currentUserRole == "Customer" && user.Id != currentUserId)
                    throw new UnauthorizedAccessException("You don't have permission to view this reservation");

                var reservations = await _reservationRepository.GetByConditionAsync(u=>u.UserId==request.UserId, cancellationToken);

                return reservations.Select(r => new ReservationDto(
                    r.Id,
                    r.Name,
                    r.GuestsCount.Value,
                    r.ReservationTime.Start,
                    r.ReservationTime.End,
                    r.Wish,
                    r.Status.ToString(),
                    r.TableId,
                    r.UserId,
                    r.Version
                    )).ToList();
            }
        }
    }
}
