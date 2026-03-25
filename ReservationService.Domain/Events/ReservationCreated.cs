using ReservationService.Domain.Common;
using ReservationService.Domain.Reservations.Enums;

namespace ReservationService.Domain.Events
{
    public class ReservationCreated : DomainEvent
    {
        public int ReservationId { get; }
        public string Name { get; }
        public int GuestsCount { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public string Wish { get; }
        public int TableId { get; }
        public int UserId { get; }
        public ReservationStatus Status { get; }

        public ReservationCreated(
        int reservationId,
        string name,
        int guestsCount,
        DateTime startTime,
        DateTime endTime,
        string wish,
        int tableId,
        int userId,
        ReservationStatus status)
        {
            ReservationId = reservationId;
            Name = name;
            GuestsCount = guestsCount;
            StartTime = startTime;
            EndTime = endTime;
            Wish = wish;
            TableId = tableId;
            UserId = userId;
            Status = status;
        }
    }
}
