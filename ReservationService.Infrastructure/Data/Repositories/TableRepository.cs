using Microsoft.EntityFrameworkCore;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Reservations.ValueObjects;
using ReservationService.Domain.Tables;

namespace ReservationService.Infrastructure.Data.Repositories
{
    public class TableRepository : Repository<Table>, ITableRepository
    {
        public TableRepository(ApplicationDbContext context) : base(context)
        {
            
        }

        public async Task<IEnumerable<Table>> GetAvailableTablesAsync(TimeRange timeRange, int guestsCount, CancellationToken cancellationToken = default)
        {
            var sql = @"
        SELECT t.* FROM public.""Tables"" t
        WHERE t.""Capacity"" >= {0}
        AND NOT EXISTS (
            SELECT 1 FROM public.""Reservations"" r
            WHERE r.table_id = t.""Id""
            AND r.status != 'Cancelled'
            AND r.start_time < {2}
            AND {1} < r.end_time
        )";

            return await _context.Tables
        .FromSqlRaw(sql, guestsCount, timeRange.Start, timeRange.End)
        .ToListAsync(cancellationToken);
        }
    }
}
