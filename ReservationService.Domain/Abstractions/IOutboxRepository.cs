using ReservationService.Domain.Common;

namespace ReservationService.Domain.Abstractions
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
        Task<IEnumerable<OutboxMessage>> GetUnpublishedMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default);
        Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    }
}
