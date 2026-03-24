namespace ReservationService.Application.DTOs
{
    public record ReservationDto(int Id, string Name, int GuestsCount, DateTime StartTime,
        DateTime EndTime, string? Wish, string ReservationStatus, int TableId, int UserId, long Version);
}
