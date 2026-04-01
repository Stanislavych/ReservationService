using ReservationService.Domain.Common;
using ReservationService.Domain.Users.ValueObjects;

namespace ReservationService.Domain.Users
{
    public class User : AggregateRoot
    {
        public int Id { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public PhoneNumber PhoneNumber { get; private set; } = null!;
        public string Role { get; private set; } = string.Empty;

        private User()
        {

        }

        public static User Create(string firstName, string lastName, string username, string email, string passwordHash
            , PhoneNumber phoneNumber, string role = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new Exception("First name is required");
            if (string.IsNullOrWhiteSpace(lastName))
                throw new Exception("Last name is required");
            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("Uname is required");
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email is required");
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new Exception("Password is required");

            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Role = role ?? "Customer"
            };
        }

        public bool VerifyPassword(string password)
        {
            return PasswordHash == password;
        }
    }
}
