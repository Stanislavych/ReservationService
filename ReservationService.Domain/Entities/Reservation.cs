using ReservationService.Domain.Enums;
using ReservationService.Domain.ValueObjects;

namespace ReservationService.Domain.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public GuestsCount GuestsCount { get; set; } = null!;
        public TimeRange ReservationTime { get; set; } = null!;
        public string Wish { get; set; } = string.Empty;
        public ReservationStatus Status { get; set; }

        public int TableId { get; set; }
        public Table Table { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}