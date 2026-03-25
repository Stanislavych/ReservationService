using ReservationService.Domain.Common;
using ReservationService.Domain.Reservations.Enums;

namespace ReservationService.Domain.Events
{
    public class ReservationCancelled : DomainEvent
    {
        public int ReservationId { get; }
        public DateTime CancelledAt { get; }
        public ReservationStatus Status { get; }

        public ReservationCancelled(int reservationId, DateTime cancelledAt, ReservationStatus status)
        {
            ReservationId = reservationId;
            CancelledAt = cancelledAt;
            Status = status;
        }
    }
}
