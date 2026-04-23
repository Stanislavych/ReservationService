namespace ReservationService.Application.DTOs.Auth
{
    public record RegisterRequest(string FirstName, string LastName, string Username, string Password, string Email, string PhoneNumber);
}
