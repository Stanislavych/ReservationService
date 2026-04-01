using ReservationService.Domain.Users;
using System.Security.Claims;

namespace ReservationService.Application.Interfaces
{
    public interface IJWTService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
