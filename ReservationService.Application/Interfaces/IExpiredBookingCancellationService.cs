namespace ReservationService.Application.Interfaces
{
    public interface IExpiredBookingCancellationService
    {
        Task CancelExpiredBookingsAsync(CancellationToken cancellationToken = default);
    }
}
