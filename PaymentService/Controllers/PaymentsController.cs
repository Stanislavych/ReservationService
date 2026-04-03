using Microsoft.AspNetCore.Mvc;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ILogger<PaymentsController> logger)
        {
            _logger = logger;
        }

        [HttpPost("confirm")]
        public async Task<ActionResult<PaymentResponse>> ConfirmPayment([FromBody] PaymentRequest request)
        {
            _logger.LogInformation("Processing payment for Reservation {ReservationId}, Amount: {Amount}{Currency}",
                request.ReservationId, request.Amount, request.Currency);

            await Task.Delay(100);

            var response = new PaymentResponse
            {
                PaymentId = Guid.NewGuid(),
                ReservationId = request.ReservationId,
                IsSuccessful = true,
                Status = "Completed",
                Message = "Payment processed successfully",
                ProcessedAt = DateTime.UtcNow
            };

            return Ok(response);
        }

        [HttpGet("health")]
        public IActionResult Health() => Ok(new { status = "healthy" });
    }
}
