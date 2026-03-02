using ReservationService.Domain.Enums;

namespace ReservationService.Domain.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int GuestsCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Wish { get; set; } = string.Empty;
        public ReservationStatus Status { get; set; }

        public int TableId { get; set; }
        public Table Table { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
