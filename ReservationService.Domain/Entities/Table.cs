using ReservationService.Domain.Enums;
using ReservationService.Domain.ValueObjects;

namespace ReservationService.Domain.Entities
{
    public class Table
    {
        public int Id { get; set; }
        public TableNumber Number { get; set; } = null!;
        public TableType Type { get; set; }
        public TableZone Zone { get; set; }
        public Capacity Capacity { get; set; } = null!;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
