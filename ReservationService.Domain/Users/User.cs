using ReservationService.Domain.Common;
using ReservationService.Domain.Users.ValueObjects;

namespace ReservationService.Domain.Users
{
    public class User : AggregateRoot
    {
        public int Id { get; private set; }
        public string FirstName { get;private set; } = string.Empty;
        public string LastName { get;private set; } = string.Empty;
        public PhoneNumber PhoneNumber { get; private set; } = null!;
        public string Role { get; private set; } = string.Empty;

        private User()
        {
            
        }

        public static User Create(string firstName, string lastName, PhoneNumber phoneNumber, string role)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new Exception("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new Exception("Last name is required");

            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Role = role ?? "Customer"
            };
        }
    }
}
