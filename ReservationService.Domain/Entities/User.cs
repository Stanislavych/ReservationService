using ReservationService.Domain.ValueObjects;

namespace ReservationService.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public PhoneNumber PhoneNumber { get; set; } = null!;
        public string Role { get; set; } = string.Empty;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
