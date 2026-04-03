using MediatR;

namespace ReservationService.Application.Commands
{
    public interface IIdempotentRequest<out TResponse> : IRequest<TResponse>
    {
        string IdempotencyKey { get; }
    }

    public abstract record IdempotentCommand<TResponse> : IIdempotentRequest<TResponse>
    {
        public required string IdempotencyKey { get; init; }
    }
}
