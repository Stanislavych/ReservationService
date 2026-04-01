using ReservationService.Domain.Users;
using ReservationService.Domain.Users.ValueObjects;

namespace ReservationService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(string firstName, string lastName, string username, string password, string email, string phoneNumber);
    }
}
