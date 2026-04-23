namespace PaymentService.Models
{
    public class PaymentRequest
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "RUB";
        public string PaymentMethod { get; set; } = "card";
    }
}
