using ReservationService.Domain.Common;
using ReservationService.Domain.Reservations.Enums;

namespace ReservationService.Domain.Events
{
    public class ReservationCompleted : DomainEvent
    {
        public int ReservationId { get; }
        public DateTime CompletedAt { get; }
        public ReservationStatus Status { get; }

        public ReservationCompleted(int reservationId, DateTime completedAt, ReservationStatus status)
        {
            ReservationId = reservationId;
            CompletedAt = completedAt;
            Status = status;
        }
    }
}
