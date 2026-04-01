using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using ReservationService.Domain.Events;

namespace ReservationService.Infrastructure.Consumers
{
    public class ReservationCreatedConsumer : IConsumer<ReservationCreated>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ReservationCreatedConsumer> _logger;

        public ReservationCreatedConsumer(IMediator mediator, ILogger<ReservationCreatedConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReservationCreated> context)
        {
            _logger.LogInformation("Received ReservationCreated event: {ReservationId}", context.Message.ReservationId);

            await _mediator.Publish(context.Message, context.CancellationToken);
        }
    }
}
