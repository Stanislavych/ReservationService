namespace PaymentService.Models
{
    public class PaymentResponse
    {
        public Guid PaymentId { get; set; }
        public int ReservationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
