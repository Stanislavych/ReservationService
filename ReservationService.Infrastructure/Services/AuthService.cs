using ReservationService.Application.Interfaces;
using ReservationService.Domain.Users;
using ReservationService.Domain.Users.ValueObjects;
using System.Collections.Concurrent;

namespace ReservationService.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private static readonly ConcurrentDictionary<string, User> _users = new();

        static AuthService()
        {
            var adminUser = User.Create("admin", "admin", "admin","admin@admin.com","admin",PhoneNumber.Create("7777777"),"admin");
            var testUser = User.Create("test", "test", "test","test@test.com","test",PhoneNumber.Create("11111111"));

            _users.TryAdd(adminUser.Username, adminUser);
            _users.TryAdd(testUser.Username, testUser);
        }

        public Task<User?> AuthenticateAsync(string username, string password)
        {
            if (_users.TryGetValue(username, out var user) && user.VerifyPassword(password))
            {
                return Task.FromResult<User?>(user);
            }

            return Task.FromResult<User?>(null);
        }

        public Task<User> RegisterAsync(string firstName, string lastName, string username, string password, string email, string phoneNumber)
        {
            if (_users.ContainsKey(username))
                throw new InvalidOperationException("Username already exists");

            var user = User.Create(firstName, lastName, username, email, password, PhoneNumber.Create(phoneNumber));

            _users.TryAdd(username, user);
            
            return Task.FromResult(user);
        }
    }
}
