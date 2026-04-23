using ReservationService.Application.DTOs.Payment;

namespace ReservationService.Application.Interfaces
{
    public interface IPaymentServiceClient
    {
        Task<PaymentResponse> ConfirmPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
        Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
    }
}
