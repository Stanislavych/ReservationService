namespace ReservationService.Infrastructure.Data.Configurations
{
    public class PaymentServiceSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:8080";
        public int TimeoutSeconds { get; set; } = 30;
        public bool Enabled { get; set; } = true;
        public string ConfirmEndpoint { get; set; } = "/api/payments/confirm";
    }
}
