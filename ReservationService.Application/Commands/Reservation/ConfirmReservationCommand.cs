using MediatR;
using Microsoft.AspNetCore.Http;
using ReservationService.Application.Base;
using ReservationService.Application.DTOs;
using ReservationService.Application.DTOs.Payment;
using ReservationService.Application.Interfaces;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;
using ReservationService.Domain.Exceptions;
using ReservationService.Domain.Reservations;
using System.Text.Json;

namespace ReservationService.Application.Commands.Reservation
{
    public record ConfirmReservationCommand(int Id, long Version) : IRequest<ReservationDto>
    {
        public class Handler : BaseHandler<ConfirmReservationCommand, ReservationDto>
        {
            private readonly IReservationUpdateService _reservationUpdateService;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IOutboxRepository _outboxRepository;
            private readonly IReservationRepository _reservationRepository;
            private readonly IPaymentServiceClient _paymentServiceClient;

            public Handler(IReservationUpdateService reservationUpdateService, IUnitOfWork unitOfWork,
                IOutboxRepository outboxRepository, IHttpContextAccessor httpContextAccessor, IReservationRepository reservationRepository, IPaymentServiceClient paymentServiceClient) : base(httpContextAccessor)
            {
                _reservationUpdateService = reservationUpdateService;
                _unitOfWork = unitOfWork;
                _outboxRepository = outboxRepository;
                _reservationRepository = reservationRepository;
                _paymentServiceClient = paymentServiceClient;
            }
            public override async Task<ReservationDto> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();
                var reservation = await _reservationRepository.GetByIdAsync(request.Id, cancellationToken);

                if (reservation == null)
                    throw new NotFoundException($"Reservation {request.Id} not found");

                var paymentRequest = new PaymentRequest
                {
                    ReservationId = reservation.Id,
                    UserId = reservation.Id,
                    Amount = CalculateAmount(reservation),
                    Currency = "RUB",
                    PaymentMethod = "card"
                };

                var paymentResponse = await _paymentServiceClient.ConfirmPaymentAsync(paymentRequest, cancellationToken);

                if (!paymentResponse.IsSuccessful)
                    throw new DomainException($"Payment failed: {paymentResponse.Message}");

                var updatedReservation = await _reservationUpdateService.ConfirmAsync(reservation, request.Version, currentUserId, currentUserRole, cancellationToken);

                foreach (var @event in updatedReservation.DomainEvents)
                {
                    var outboxMessage = new OutboxMessage(
                        @event.GetType().Name,
                        JsonSerializer.Serialize(@event, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));

                    await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync();

                updatedReservation.ClearDomainEvents();

                return new ReservationDto(
                    updatedReservation.Id,
                    updatedReservation.Name,
                    updatedReservation.GuestsCount.Value,
                    updatedReservation.ReservationTime.Start,
                    updatedReservation.ReservationTime.End,
                    updatedReservation.Wish,
                    updatedReservation.Status.ToString(),
                    updatedReservation.TableId,
                    updatedReservation.UserId,
                    updatedReservation.Version
                    );
            }

            private decimal CalculateAmount(Domain.Reservations.Reservation reservation)
            {
                var basePrice = 1000m;
                var hours = (reservation.ReservationTime.End - reservation.ReservationTime.Start).TotalHours;

                return basePrice * (decimal)hours;
            }
        }
    }
}
