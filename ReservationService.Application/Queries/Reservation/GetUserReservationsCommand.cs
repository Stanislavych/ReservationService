using MediatR;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Users;

namespace ReservationService.Application.Queries.Reservation
{
    public record GetUserReservationsCommand(int UserId) : IRequest<List<ReservationDto>>
    {
        public class Handler(IReservationRepository reservationRepository, IRepository<User> userRepository) : IRequestHandler<GetUserReservationsCommand, List<ReservationDto>>
        {
            public async Task<List<ReservationDto>> Handle(GetUserReservationsCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

                if (user == null)
                    throw new ArgumentException($"Reservations for user with id {request.UserId} not found!");

                var reservations = await reservationRepository.GetByConditionAsync(u=>u.UserId==request.UserId, cancellationToken);

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
