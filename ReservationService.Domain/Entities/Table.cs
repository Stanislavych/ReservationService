using ReservationService.Domain.Enums;

namespace ReservationService.Domain.Entities
{
    public class Table
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public TableType Type { get; set; }
        public TableZone Zone { get; set; }
        public int Capacity { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
