using ReservationService.Domain.Reservations.ValueObjects;
using ReservationService.Domain.Tables;

namespace ReservationService.Domain.Abstractions
{
    public interface ITableRepository : IRepository<Table>
    {
        Task<IEnumerable<Table>> GetAvailableTablesAsync(TimeRange timeRange, int questsCount, CancellationToken cancellationToken = default);
    }
}
