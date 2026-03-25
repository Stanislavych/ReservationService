using Microsoft.EntityFrameworkCore;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Common;

namespace ReservationService.Infrastructure.Data.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly ApplicationDbContext _context;

        public OutboxRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            await _context.Set<OutboxMessage>().AddAsync(message, cancellationToken);
        }

        public async Task<IEnumerable<OutboxMessage>> GetUnpublishedMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default)
        {
            return await _context.Set<OutboxMessage>()
                .Where(m => !m.IsPublished && m.RetryCount < 3)
                .OrderBy(m => m.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            _context.Set<OutboxMessage>().Update(message);
            
            await Task.CompletedTask;
        }
    }
}
