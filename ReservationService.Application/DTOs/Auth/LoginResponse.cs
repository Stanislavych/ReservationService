namespace ReservationService.Application.DTOs.Auth
{
    public record LoginResponse(string Token, string Username, string Role, DateTime ExpiresAt);
}
