using ReservationService.Domain.Common;

namespace ReservationService.Domain.Abstractions
{
    public interface IIdempotencyRepository
    {
        Task<IdempotencyRecord?> GetByKeyAsync(string key, string commandType, CancellationToken cancellationToken = default);
        Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);
        Task UpdateAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);
    }
}
