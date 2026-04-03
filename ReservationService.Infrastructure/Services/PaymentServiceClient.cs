using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReservationService.Application.DTOs.Payment;
using ReservationService.Application.Interfaces;
using ReservationService.Infrastructure.Data.Configurations;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ReservationService.Infrastructure.Services
{
    public class PaymentServiceClient : IPaymentServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly PaymentServiceSettings _settings;
        private readonly ILogger<PaymentServiceClient> _logger;

        public PaymentServiceClient(HttpClient httpClient, IOptions<PaymentServiceSettings> settings, ILogger<PaymentServiceClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<PaymentResponse> ConfirmPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        {
            if (!_settings.Enabled)
            {
                _logger.LogWarning("Payment service is disabled. Returning mock response");

                return new PaymentResponse
                {
                    PaymentId = Guid.NewGuid(),
                    ReservationId = request.ReservationId,
                    IsSuccessful = true,
                    Status = "Mocked",
                    Message = "Payment service disabled, mock response",
                    ProcessedAt = DateTime.UtcNow
                };
            }

            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling Payment Service: {BaseUrl}{EndPoint}", _settings.BaseUrl, _settings.ConfirmEndpoint);

                var response = await _httpClient.PostAsync(_settings.ConfirmEndpoint, content, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    throw new HttpRequestException("Payment service is unavailable");
                if (response.StatusCode == HttpStatusCode.RequestTimeout)
                    throw new TimeoutException("Payment service timeout");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Payment service error: {StatusCode}, {Response}", response.StatusCode, responseContent);

                    throw new HttpRequestException($"Payment service returned {response.StatusCode}");
                }

                var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return paymentResponse;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout calling payment service");

                throw new TimeoutException("Payment service request timed out");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling payment service");
                
                throw;
            }
        }

        public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/payments/health", cancellationToken);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
