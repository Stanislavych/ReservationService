using ReservationService.Domain.Common;
using ReservationService.Domain.Reservations.Enums;

namespace ReservationService.Domain.Events
{
    public class ReservationConfirmed : DomainEvent
    {
        public int ReservationId { get; }
        public DateTime ConfirmedAt { get; }
        public ReservationStatus Status { get; }

        public ReservationConfirmed(int reservationId, DateTime confirmedAt, ReservationStatus status)
        {
            ReservationId = reservationId;
            ConfirmedAt = confirmedAt;
            Status = status;
        }
    }
}
