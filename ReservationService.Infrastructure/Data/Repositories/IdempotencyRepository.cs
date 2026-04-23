using Microsoft.EntityFrameworkCore;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;

namespace ReservationService.Infrastructure.Data.Repositories
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly ApplicationDbContext _context;

        public IdempotencyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
        {
            await _context.Set<IdempotencyRecord>().AddAsync(record, cancellationToken);
        }

        public async Task<IdempotencyRecord?> GetByKeyAsync(string key, string commandType, CancellationToken cancellationToken = default)
        {
            return await _context.Set<IdempotencyRecord>().FirstOrDefaultAsync(x => x.Key == key && x.CommandType == commandType, cancellationToken);
        }

        public async Task UpdateAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
        {
            _context.Set<IdempotencyRecord>().Update(record);
            
            await Task.CompletedTask;
        }
    }
}
