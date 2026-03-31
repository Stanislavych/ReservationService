namespace ReservationService.Application.Interfaces
{
    public interface IMessageBroker
    {
        Task PublishAsync(string eventType, string payload, CancellationToken cancellationToken = default);
    }
}
