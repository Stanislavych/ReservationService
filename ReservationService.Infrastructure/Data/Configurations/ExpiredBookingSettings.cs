namespace ReservationService.Infrastructure.Data.Configurations
{
    public class ExpiredBookingSettings
    {
        public int PendingPaymentTimeoutMinutes { get; set; } = 5;
        public int CheckIntervalSeconds { get; set; } = 60;
    }
}
